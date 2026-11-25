using Claims.Api.Domain;

namespace Claims.Api.Tests.Domain;

/// <summary>
/// Lifecycle rules from docs/design/04-claim-lifecycle.md. Pure domain tests: no database, no Docker.
/// Names follow R{requirement}_{Method}_{Scenario}_{ExpectedBehavior}; each test has one act.
/// </summary>
public class ClaimTests
{
    private const string Description = "description";
    private const string Question = "question";
    private const string Answer = "answer";
    private const string Reason = "reason";
    private const decimal Amount = 1m;

    private static readonly DateOnly IncidentDate = new(2026, 11, 20);
    private static readonly DateTimeOffset Now = new(2026, 11, 25, 9, 0, 0, TimeSpan.Zero);
    private static readonly Guid Claimant = Guid.NewGuid();
    private static readonly Guid Officer = Guid.NewGuid();
    private static readonly Guid OtherOfficer = Guid.NewGuid();

    private static Claim Submitted() =>
        Claim.Submit(Claimant, ClaimType.Motor, IncidentDate, Description, Amount, Now);

    private static Claim UnderReview()
    {
        var claim = Submitted();
        claim.PickUp(Officer, Now);
        return claim;
    }

    private static Claim InfoRequested()
    {
        var claim = UnderReview();
        claim.RequestInformation(Officer, Question, Now);
        return claim;
    }

    private static Claim Settled()
    {
        var claim = UnderReview();
        claim.Settle(Officer, Reason, Now);
        return claim;
    }

    private static Claim Rejected()
    {
        var claim = UnderReview();
        claim.Reject(Officer, Reason, Now);
        return claim;
    }

    // R1: report an incident

    [Fact]
    public void R1_Submit_NewClaim_IsSubmittedAndUnassigned()
    {
        var claim = Submitted();

        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.Null(claim.AssigneeId);
    }

    [Fact]
    public void R1_Submit_NewClaim_GetsAClaimNumber()
    {
        var claim = Submitted();

        Assert.StartsWith("CLM-", claim.ClaimNumber);
    }

    [Fact]
    public void R1_Submit_NewClaim_RecordsTheClaimantsSubmissionAsFirstHistoryRow()
    {
        var claim = Submitted();

        var change = Assert.Single(claim.StatusHistory);
        Assert.Null(change.FromStatus);
        Assert.Equal(ClaimStatus.Submitted, change.ToStatus);
        Assert.Equal(Claimant, change.ChangedBy);
    }

    // R5: pick up an incoming claim

    [Fact]
    public void R5_PickUp_SubmittedClaim_AssignsTheOfficerAndStartsReview()
    {
        var claim = Submitted();

        claim.PickUp(Officer, Now);

        Assert.Equal(ClaimStatus.UnderReview, claim.Status);
        Assert.Equal(Officer, claim.AssigneeId);
    }

    [Fact]
    public void R5_PickUp_AlreadyPickedUpClaim_ThrowsInvalidTransition()
    {
        var claim = UnderReview();

        var act = () => claim.PickUp(OtherOfficer, Now);

        Assert.Throws<InvalidTransitionException>(act);
        Assert.Equal(Officer, claim.AssigneeId);
    }

    [Fact]
    public void R5_PickUp_SettledClaim_ThrowsInvalidTransition()
    {
        var claim = Settled();

        var act = () => claim.PickUp(Officer, Now);

        Assert.Throws<InvalidTransitionException>(act);
    }

    // R6: review and ask for information

    [Fact]
    public void R6_RequestInformation_UnderReviewClaim_MovesToInfoRequestedWithAnOpenRequest()
    {
        var claim = UnderReview();

        var request = claim.RequestInformation(Officer, Question, Now);

        Assert.Equal(ClaimStatus.InfoRequested, claim.Status);
        Assert.True(request.IsOpen);
        Assert.Same(request, Assert.Single(claim.InformationRequests));
    }

    [Fact]
    public void R6_RequestInformation_OtherOfficer_ThrowsNotPermitted()
    {
        var claim = UnderReview();

        var act = () => claim.RequestInformation(OtherOfficer, Question, Now);

        Assert.Throws<NotPermittedException>(act);
    }

    [Fact]
    public void R6_RequestInformation_ClaimNotPickedUp_ThrowsInvalidTransition()
    {
        var claim = Submitted();

        var act = () => claim.RequestInformation(Officer, Question, Now);

        Assert.Throws<InvalidTransitionException>(act);
    }

    [Fact]
    public void R6_RequestInformation_WhileARequestIsOpen_ThrowsInvalidTransition()
    {
        var claim = InfoRequested();

        var act = () => claim.RequestInformation(Officer, Question, Now);

        Assert.Throws<InvalidTransitionException>(act);
    }

    // R3: provide additional information

    [Fact]
    public void R3_ProvideInformation_OpenRequest_AnswersItAndReturnsToReview()
    {
        var claim = InfoRequested();

        claim.ProvideInformation(Claimant, Answer, Now);

        var request = Assert.Single(claim.InformationRequests);
        Assert.Equal(ClaimStatus.UnderReview, claim.Status);
        Assert.False(request.IsOpen);
        Assert.Equal(Answer, request.Response);
    }

    [Fact]
    public void R3_ProvideInformation_OtherClaimant_ThrowsNotPermitted()
    {
        var claim = InfoRequested();

        var act = () => claim.ProvideInformation(Guid.NewGuid(), Answer, Now);

        Assert.Throws<NotPermittedException>(act);
    }

    [Fact]
    public void R3_ProvideInformation_NoOpenRequest_ThrowsInvalidTransition()
    {
        var claim = UnderReview();

        var act = () => claim.ProvideInformation(Claimant, Answer, Now);

        Assert.Throws<InvalidTransitionException>(act);
    }

    [Fact]
    public void R3_ProvideInformation_SecondRound_LeavesBothRequestsAnswered()
    {
        var claim = InfoRequested();
        claim.ProvideInformation(Claimant, Answer, Now);
        claim.RequestInformation(Officer, Question, Now);

        claim.ProvideInformation(Claimant, Answer, Now);

        Assert.Equal(ClaimStatus.UnderReview, claim.Status);
        Assert.Equal(2, claim.InformationRequests.Count);
        Assert.All(claim.InformationRequests, r => Assert.False(r.IsOpen));
    }

    // R7: settle or reject

    [Fact]
    public void R7_Settle_UnderReviewClaim_RecordsTheDecision()
    {
        var claim = UnderReview();

        claim.Settle(Officer, Reason, Now);

        Assert.Equal(ClaimStatus.Settled, claim.Status);
        Assert.Equal(DecisionOutcome.Settled, claim.DecisionOutcome);
        Assert.Equal(Reason, claim.DecisionReason);
        Assert.Equal(Now, claim.DecidedAt);
    }

    [Fact]
    public void R7_Reject_UnderReviewClaim_RecordsTheDecisionAndReason()
    {
        var claim = UnderReview();

        claim.Reject(Officer, Reason, Now);

        Assert.Equal(ClaimStatus.Rejected, claim.Status);
        Assert.Equal(DecisionOutcome.Rejected, claim.DecisionOutcome);
        Assert.Equal(Reason, claim.DecisionReason);
    }

    [Fact]
    public void R7_Reject_UnderReviewClaim_KeepsTheReasonInTheHistoryNote()
    {
        var claim = UnderReview();

        claim.Reject(Officer, Reason, Now);

        Assert.Equal(Reason, claim.StatusHistory.Last().Note);
    }

    [Fact]
    public void R7_DecisionFields_UnderReviewClaim_AreEmpty()
    {
        var claim = UnderReview();

        Assert.Null(claim.DecisionOutcome);
        Assert.Null(claim.DecisionReason);
        Assert.Null(claim.DecidedAt);
    }

    [Fact]
    public void R7_Settle_OtherOfficer_ThrowsNotPermitted()
    {
        var claim = UnderReview();

        var act = () => claim.Settle(OtherOfficer, Reason, Now);

        Assert.Throws<NotPermittedException>(act);
    }

    [Fact]
    public void R7_Settle_WhileInfoIsRequested_ThrowsInvalidTransition()
    {
        var claim = InfoRequested();

        var act = () => claim.Settle(Officer, Reason, Now);

        Assert.Throws<InvalidTransitionException>(act);
    }

    [Fact]
    public void R7_Reject_SettledClaim_ThrowsInvalidTransition()
    {
        var claim = Settled();

        var act = () => claim.Reject(Officer, Reason, Now);

        Assert.Throws<InvalidTransitionException>(act);
    }

    [Fact]
    public void R7_Settle_RejectedClaim_ThrowsInvalidTransition()
    {
        var claim = Rejected();

        var act = () => claim.Settle(Officer, Reason, Now);

        Assert.Throws<InvalidTransitionException>(act);
    }

    // R2: track a claim

    [Fact]
    public void R2_StatusHistory_FullJourney_RecordsEveryTransitionInOrder()
    {
        var claim = InfoRequested();
        claim.ProvideInformation(Claimant, Answer, Now);

        claim.Settle(Officer, Reason, Now);

        Assert.Equal(
            [ClaimStatus.Submitted, ClaimStatus.UnderReview, ClaimStatus.InfoRequested, ClaimStatus.UnderReview, ClaimStatus.Settled],
            claim.StatusHistory.Select(h => h.ToStatus));
    }
}
