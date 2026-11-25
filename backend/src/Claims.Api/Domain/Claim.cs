using Claims.Api.Domain.Events;

namespace Claims.Api.Domain;

/// <summary>
/// The claim aggregate. All status changes go through the methods below, which enforce the lifecycle in
/// docs/design/04-claim-lifecycle.md and record a <see cref="ClaimStatusChange"/> for each one.
/// </summary>
public sealed class Claim
{
    private readonly List<ClaimStatusChange> _statusHistory = [];
    private readonly List<InformationRequest> _informationRequests = [];
    private readonly List<ClaimDomainEvent> _domainEvents = [];

    private Claim()
    {
    }

    public Guid Id { get; private set; }

    public string ClaimNumber { get; private set; } = string.Empty;

    public Guid ClaimantId { get; private set; }

    /// <summary>The officer working on the claim. Null until it is picked up.</summary>
    public Guid? AssigneeId { get; private set; }

    public ClaimType Type { get; private set; }

    public ClaimStatus Status { get; private set; }

    public DateOnly IncidentDate { get; private set; }

    public string Description { get; private set; } = string.Empty;

    public decimal ClaimedAmount { get; private set; }

    public DecisionOutcome? DecisionOutcome { get; private set; }

    public string? DecisionReason { get; private set; }

    public DateTimeOffset? DecidedAt { get; private set; }

    public DateTimeOffset SubmittedAt { get; private set; }

    /// <summary>Optimistic concurrency token (PostgreSQL xmin). Stops two officers picking up the same claim.</summary>
    public uint Version { get; private set; }

    public IReadOnlyCollection<ClaimStatusChange> StatusHistory => _statusHistory;

    public IReadOnlyCollection<InformationRequest> InformationRequests => _informationRequests;

    /// <summary>Events raised since the last save. The DbContext moves them to the outbox and clears them (RFC-0002).</summary>
    public IReadOnlyCollection<ClaimDomainEvent> DomainEvents => _domainEvents;

    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>R1: a claimant reports an incident.</summary>
    public static Claim Submit(
        Guid claimantId,
        ClaimType type,
        DateOnly incidentDate,
        string description,
        decimal claimedAmount,
        DateTimeOffset now)
    {
        var claim = new Claim
        {
            Id = Guid.CreateVersion7(),
            ClaimNumber = $"CLM-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
            ClaimantId = claimantId,
            Type = type,
            IncidentDate = incidentDate,
            Description = description,
            ClaimedAmount = claimedAmount,
            SubmittedAt = now,
        };
        claim.MoveTo(ClaimStatus.Submitted, claimantId, now, note: null);
        claim.Raise(new ClaimSubmitted(claim.ClaimNumber, claimantId, type, incidentDate, claimedAmount), now);
        return claim;
    }

    /// <summary>
    /// Rebuilds a claim that already has a history (sample data import, ADR-0021). It checks no lifecycle rules and
    /// raises no events: the facts happened before this system saw them.
    /// </summary>
    internal static Claim Restore(
        Guid id,
        string claimNumber,
        Guid claimantId,
        Guid? assigneeId,
        ClaimType type,
        ClaimStatus status,
        DateOnly incidentDate,
        string description,
        decimal claimedAmount,
        DecisionOutcome? decisionOutcome,
        string? decisionReason,
        DateTimeOffset? decidedAt,
        DateTimeOffset submittedAt,
        IEnumerable<ClaimStatusChange> statusHistory,
        IEnumerable<InformationRequest> informationRequests)
    {
        var claim = new Claim
        {
            Id = id,
            ClaimNumber = claimNumber,
            ClaimantId = claimantId,
            AssigneeId = assigneeId,
            Type = type,
            Status = status,
            IncidentDate = incidentDate,
            Description = description,
            ClaimedAmount = claimedAmount,
            DecisionOutcome = decisionOutcome,
            DecisionReason = decisionReason,
            DecidedAt = decidedAt,
            SubmittedAt = submittedAt,
        };
        claim._statusHistory.AddRange(statusHistory);
        claim._informationRequests.AddRange(informationRequests);
        return claim;
    }

    /// <summary>R5: staff pick up an incoming claim.</summary>
    public void PickUp(Guid officerId, DateTimeOffset now)
    {
        EnsureStatus(ClaimStatus.Submitted, "picked up");
        AssigneeId = officerId;
        MoveTo(ClaimStatus.UnderReview, officerId, now, note: null);
        Raise(new ClaimPickedUp(officerId), now);
    }

    /// <summary>R6: the assigned officer asks the claimant for more information.</summary>
    public InformationRequest RequestInformation(Guid officerId, string question, DateTimeOffset now)
    {
        EnsureStatus(ClaimStatus.UnderReview, "asked for information");
        EnsureAssignee(officerId);

        var request = new InformationRequest(Id, officerId, question, now);
        _informationRequests.Add(request);
        MoveTo(ClaimStatus.InfoRequested, officerId, now, note: null);
        Raise(new InformationRequested(request.Id, question), now);
        return request;
    }

    /// <summary>R3: the claimant answers the open information request.</summary>
    public void ProvideInformation(Guid claimantId, string response, DateTimeOffset now)
    {
        if (claimantId != ClaimantId)
        {
            throw new NotPermittedException("Only the claimant can provide information for this claim.");
        }

        EnsureStatus(ClaimStatus.InfoRequested, "answered");

        // Requesting information needs UnderReview and moves the claim to InfoRequested, so exactly one request is open.
        var request = _informationRequests.Single(r => r.IsOpen);
        request.Respond(response, now);
        MoveTo(ClaimStatus.UnderReview, claimantId, now, note: null);
        Raise(new InformationProvided(request.Id), now);
    }

    /// <summary>R7: the assigned officer settles the claim.</summary>
    public void Settle(Guid officerId, string? reason, DateTimeOffset now) =>
        Decide(officerId, ClaimStatus.Settled, Domain.DecisionOutcome.Settled, reason, now);

    /// <summary>R7: the assigned officer rejects the claim.</summary>
    public void Reject(Guid officerId, string reason, DateTimeOffset now) =>
        Decide(officerId, ClaimStatus.Rejected, Domain.DecisionOutcome.Rejected, reason, now);

    private void Decide(Guid officerId, ClaimStatus target, DecisionOutcome outcome, string? reason, DateTimeOffset now)
    {
        EnsureStatus(ClaimStatus.UnderReview, "decided");
        EnsureAssignee(officerId);

        DecisionOutcome = outcome;
        DecisionReason = reason;
        DecidedAt = now;
        MoveTo(target, officerId, now, note: reason);
        Raise(outcome == Domain.DecisionOutcome.Settled ? new ClaimSettled(reason) : new ClaimRejected(reason!), now);
    }

    private void Raise(ClaimDomainEvent @event, DateTimeOffset now) =>
        _domainEvents.Add(@event with { ClaimId = Id, OccurredAt = now });

    private void EnsureStatus(ClaimStatus required, string action)
    {
        if (Status != required)
        {
            throw new InvalidTransitionException($"Claim {ClaimNumber} is {Status} and cannot be {action}.");
        }
    }

    private void EnsureAssignee(Guid officerId)
    {
        if (AssigneeId != officerId)
        {
            throw new NotPermittedException($"Only the officer assigned to claim {ClaimNumber} can do this.");
        }
    }

    private void MoveTo(ClaimStatus target, Guid changedBy, DateTimeOffset now, string? note)
    {
        ClaimStatus? from = _statusHistory.Count == 0 ? null : Status;
        Status = target;
        _statusHistory.Add(new ClaimStatusChange(Id, from, target, changedBy, now, note));
    }
}
