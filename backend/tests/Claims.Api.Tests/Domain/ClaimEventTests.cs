using Claims.Api.Domain;
using Claims.Api.Domain.Events;

namespace Claims.Api.Tests.Domain;

/// <summary>RFC-0002 section 3: each lifecycle transition raises one past-tense event. Pure domain tests.</summary>
public class ClaimEventTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 25, 9, 0, 0, TimeSpan.Zero);
    private static readonly Guid Claimant = Guid.NewGuid();
    private static readonly Guid Officer = Guid.NewGuid();

    private static Claim Submitted() =>
        Claim.Submit(Claimant, ClaimType.Motor, new DateOnly(2026, 11, 20), "description", 12.5m, Now);

    private static Claim UnderReview()
    {
        var claim = Submitted();
        claim.PickUp(Officer, Now);
        claim.ClearDomainEvents();
        return claim;
    }

    private static Claim InfoRequested()
    {
        var claim = UnderReview();
        claim.RequestInformation(Officer, "question", Now);
        claim.ClearDomainEvents();
        return claim;
    }

    [Fact]
    public void C3_Submit_NewClaim_RaisesClaimSubmitted()
    {
        var claim = Submitted();

        var raised = Assert.IsType<ClaimSubmitted>(Assert.Single(claim.DomainEvents));
        Assert.Equal(claim.Id, raised.ClaimId);
        Assert.Equal(Now, raised.OccurredAt);
        Assert.Equal(claim.ClaimNumber, raised.ClaimNumber);
        Assert.Equal(Claimant, raised.ClaimantId);
        Assert.Equal(12.5m, raised.ClaimedAmount);
    }

    [Fact]
    public void C3_PickUp_SubmittedClaim_RaisesClaimPickedUp()
    {
        var claim = Submitted();
        claim.ClearDomainEvents();

        claim.PickUp(Officer, Now);

        var raised = Assert.IsType<ClaimPickedUp>(Assert.Single(claim.DomainEvents));
        Assert.Equal(Officer, raised.AssigneeId);
    }

    [Fact]
    public void C3_RequestInformation_UnderReviewClaim_RaisesInformationRequested()
    {
        var claim = UnderReview();

        var request = claim.RequestInformation(Officer, "question", Now);

        var raised = Assert.IsType<InformationRequested>(Assert.Single(claim.DomainEvents));
        Assert.Equal(request.Id, raised.InformationRequestId);
        Assert.Equal("question", raised.Question);
    }

    [Fact]
    public void C3_ProvideInformation_OpenRequest_RaisesInformationProvided()
    {
        var claim = InfoRequested();
        var requestId = claim.InformationRequests.Single().Id;

        claim.ProvideInformation(Claimant, "answer", Now);

        var raised = Assert.IsType<InformationProvided>(Assert.Single(claim.DomainEvents));
        Assert.Equal(requestId, raised.InformationRequestId);
    }

    [Fact]
    public void C3_Settle_UnderReviewClaim_RaisesClaimSettled()
    {
        var claim = UnderReview();

        claim.Settle(Officer, "reason", Now);

        var raised = Assert.IsType<ClaimSettled>(Assert.Single(claim.DomainEvents));
        Assert.Equal("reason", raised.Reason);
    }

    [Fact]
    public void C3_Reject_UnderReviewClaim_RaisesClaimRejected()
    {
        var claim = UnderReview();

        claim.Reject(Officer, "reason", Now);

        var raised = Assert.IsType<ClaimRejected>(Assert.Single(claim.DomainEvents));
        Assert.Equal("reason", raised.Reason);
    }

    [Fact]
    public void C3_PickUp_ClaimAlreadyPickedUp_RaisesNoEvent()
    {
        var claim = UnderReview();

        Assert.Throws<InvalidTransitionException>(() => claim.PickUp(Officer, Now));

        Assert.Empty(claim.DomainEvents);
    }
}
