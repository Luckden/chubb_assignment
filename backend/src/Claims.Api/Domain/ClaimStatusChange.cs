namespace Claims.Api.Domain;

/// <summary>One row per status change of a claim, so tracking and performance can be derived.</summary>
public sealed class ClaimStatusChange
{
    private ClaimStatusChange()
    {
    }

    internal ClaimStatusChange(Guid claimId, ClaimStatus? from, ClaimStatus to, Guid changedBy, DateTimeOffset changedAt, string? note)
    {
        Id = Guid.CreateVersion7();
        ClaimId = claimId;
        FromStatus = from;
        ToStatus = to;
        ChangedBy = changedBy;
        ChangedAt = changedAt;
        Note = note;
    }

    public Guid Id { get; private set; }

    public Guid ClaimId { get; private set; }

    /// <summary>Null for the first change, when the claim is created.</summary>
    public ClaimStatus? FromStatus { get; private set; }

    public ClaimStatus ToStatus { get; private set; }

    public Guid ChangedBy { get; private set; }

    public DateTimeOffset ChangedAt { get; private set; }

    public string? Note { get; private set; }
}
