namespace Claims.Api.Domain;

/// <summary>A question from staff to the claimant, and the claimant's answer.</summary>
public sealed class InformationRequest
{
    private InformationRequest()
    {
    }

    internal InformationRequest(Guid claimId, Guid requestedBy, string question, DateTimeOffset requestedAt)
    {
        Id = Guid.CreateVersion7();
        ClaimId = claimId;
        RequestedBy = requestedBy;
        Question = question;
        RequestedAt = requestedAt;
    }

    public Guid Id { get; private set; }

    public Guid ClaimId { get; private set; }

    public Guid RequestedBy { get; private set; }

    public string Question { get; private set; } = string.Empty;

    public string? Response { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? RespondedAt { get; private set; }

    public bool IsOpen => RespondedAt is null;

    internal void Respond(string response, DateTimeOffset respondedAt)
    {
        Response = response;
        RespondedAt = respondedAt;
    }
}
