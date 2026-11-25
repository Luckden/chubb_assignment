using Claims.Api.Domain;

namespace Claims.Api.Tests.Domain;

/// <summary>Lifecycle rules from docs/design/04-claim-lifecycle.md. Pure domain tests: no database, no Docker.</summary>
public class ClaimTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 25, 9, 0, 0, TimeSpan.Zero);
    private static readonly Guid Claimant = Guid.NewGuid();
    private static readonly Guid Officer = Guid.NewGuid();
    private static readonly Guid OtherOfficer = Guid.NewGuid();

    private static Claim NewClaim() =>
        Claim.Submit(Claimant, ClaimType.Motor, new DateOnly(2026, 11, 20), "Rear-ended at a junction", 1200m, Now);

    private static Claim UnderReview()
    {
        var claim = NewClaim();
        claim.PickUp(Officer, Now);
        return claim;
    }

    [Fact]
    public void R1_Submit_creates_a_submitted_claim_with_one_history_row()
    {
        var claim = NewClaim();

        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.Null(claim.AssigneeId);
        Assert.StartsWith("CLM-", claim.ClaimNumber);
        var change = Assert.Single(claim.StatusHistory);
        Assert.Null(change.FromStatus);
        Assert.Equal(ClaimStatus.Submitted, change.ToStatus);
        Assert.Equal(Claimant, change.ChangedBy);
    }

    [Fact]
    public void R5_PickUp_assigns_the_officer_and_starts_review()
    {
        var claim = NewClaim();

        claim.PickUp(Officer, Now);

        Assert.Equal(ClaimStatus.UnderReview, claim.Status);
        Assert.Equal(Officer, claim.AssigneeId);
        Assert.Equal(2, claim.StatusHistory.Count);
    }

    [Fact]
    public void R5_PickUp_twice_is_rejected()
    {
        var claim = UnderReview();

        Assert.Throws<InvalidTransitionException>(() => claim.PickUp(OtherOfficer, Now));
        Assert.Equal(Officer, claim.AssigneeId);
    }

    [Fact]
    public void R6_RequestInformation_moves_to_InfoRequested_and_adds_an_open_request()
    {
        var claim = UnderReview();

        var request = claim.RequestInformation(Officer, "Please send the police report", Now);

        Assert.Equal(ClaimStatus.InfoRequested, claim.Status);
        Assert.True(request.IsOpen);
        Assert.Same(request, Assert.Single(claim.InformationRequests));
    }

    [Fact]
    public void R6_RequestInformation_by_another_officer_is_not_permitted()
    {
        var claim = UnderReview();

        Assert.Throws<NotPermittedException>(() => claim.RequestInformation(OtherOfficer, "Question?", Now));
    }

    [Fact]
    public void R6_RequestInformation_before_pick_up_is_rejected()
    {
        var claim = NewClaim();

        Assert.Throws<InvalidTransitionException>(() => claim.RequestInformation(Officer, "Question?", Now));
    }

    [Fact]
    public void R3_ProvideInformation_answers_the_request_and_returns_to_review()
    {
        var claim = UnderReview();
        var request = claim.RequestInformation(Officer, "Please send the police report", Now);

        claim.ProvideInformation(Claimant, "Attached: report 4411", Now.AddHours(1));

        Assert.Equal(ClaimStatus.UnderReview, claim.Status);
        Assert.False(request.IsOpen);
        Assert.Equal("Attached: report 4411", request.Response);
    }

    [Fact]
    public void R3_ProvideInformation_by_someone_else_is_not_permitted()
    {
        var claim = UnderReview();
        claim.RequestInformation(Officer, "Question?", Now);

        Assert.Throws<NotPermittedException>(() => claim.ProvideInformation(Guid.NewGuid(), "Answer", Now));
    }

    [Fact]
    public void R3_ProvideInformation_without_an_open_request_is_rejected()
    {
        var claim = UnderReview();

        Assert.Throws<InvalidTransitionException>(() => claim.ProvideInformation(Claimant, "Answer", Now));
    }

    [Fact]
    public void R7_Settle_records_the_decision()
    {
        var claim = UnderReview();

        claim.Settle(Officer, "Covered under section 4", Now.AddDays(1));

        Assert.Equal(ClaimStatus.Settled, claim.Status);
        Assert.Equal(DecisionOutcome.Settled, claim.DecisionOutcome);
        Assert.Equal("Covered under section 4", claim.DecisionReason);
        Assert.Equal(Now.AddDays(1), claim.DecidedAt);
    }

    [Fact]
    public void R7_Reject_records_the_decision_and_reason()
    {
        var claim = UnderReview();

        claim.Reject(Officer, "Policy had lapsed", Now.AddDays(1));

        Assert.Equal(ClaimStatus.Rejected, claim.Status);
        Assert.Equal(DecisionOutcome.Rejected, claim.DecisionOutcome);
        Assert.Equal("Policy had lapsed", claim.DecisionReason);
    }

    [Fact]
    public void R7_Decision_by_another_officer_is_not_permitted()
    {
        var claim = UnderReview();

        Assert.Throws<NotPermittedException>(() => claim.Settle(OtherOfficer, null, Now));
    }

    [Fact]
    public void R7_Decision_while_waiting_for_information_is_rejected()
    {
        var claim = UnderReview();
        claim.RequestInformation(Officer, "Question?", Now);

        Assert.Throws<InvalidTransitionException>(() => claim.Settle(Officer, null, Now));
    }

    [Fact]
    public void R7_Settled_and_rejected_claims_are_final()
    {
        var settled = UnderReview();
        settled.Settle(Officer, null, Now);
        var rejected = UnderReview();
        rejected.Reject(Officer, "Not covered", Now);

        Assert.Throws<InvalidTransitionException>(() => settled.Reject(Officer, "Changed my mind", Now));
        Assert.Throws<InvalidTransitionException>(() => rejected.Settle(Officer, null, Now));
        Assert.Throws<InvalidTransitionException>(() => settled.PickUp(Officer, Now));
    }

    [Fact]
    public void R2_Every_transition_is_recorded_in_order()
    {
        var claim = UnderReview();
        claim.RequestInformation(Officer, "Question?", Now);
        claim.ProvideInformation(Claimant, "Answer", Now);
        claim.Settle(Officer, null, Now);

        var path = claim.StatusHistory.Select(h => h.ToStatus).ToArray();

        Assert.Equal(
            [ClaimStatus.Submitted, ClaimStatus.UnderReview, ClaimStatus.InfoRequested, ClaimStatus.UnderReview, ClaimStatus.Settled],
            path);
    }
}
