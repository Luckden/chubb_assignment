using Claims.Api.Domain;
using Claims.Api.Infrastructure.Persistence.Seeders;

namespace Claims.Api.Tests;

/// <summary>
/// The synthetic data set maps onto the five-status model (ADR-0021). These tests read the embedded CSV files and need no
/// database. They support the work-queue and exposure views (D7) with realistic volume.
/// </summary>
public class SampleDataTests
{
    private const int ClaimCount = 1386;

    private static readonly SampleData Data = SampleDataLoader.Load();

    [Fact]
    public void D7_Load_BundledFiles_AssignsEveryClaimToSampleClaimantAndSampleStaff()
    {
        Assert.Equal(ClaimCount, Data.Claims.Count);
        Assert.All(Data.Claims, c => Assert.Equal(DemoUsers.SampleClaimant, c.ClaimantId));
        Assert.All(
            Data.Claims.Where(c => c.AssigneeId is not null),
            c => Assert.Equal(DemoUsers.SampleStaff, c.AssigneeId));
        Assert.Contains(
            Data.Claims,
            c => c.Status == ClaimStatus.Rejected && c.AssigneeId == DemoUsers.SampleStaff);
    }

    [Fact]
    public void D7_ClaimsToSeed_TwoOfEachStatus_ForTheDemoUsersAndSampleUsers()
    {
        var sample = SampleDataSeeder.ClaimsToSeed(Data.Claims);

        Assert.Equal(SampleDataSeeder.PerStatus * Enum.GetValues<ClaimStatus>().Length, sample.Count);
        Assert.All(Enum.GetValues<ClaimStatus>(), status =>
            Assert.Equal(SampleDataSeeder.PerStatus, sample.Count(c => c.Status == status)));
        Assert.All(sample, c => Assert.Equal(DemoUsers.SampleClaimant, c.ClaimantId));
        Assert.All(sample.Where(c => c.Status == ClaimStatus.Submitted), c => Assert.Null(c.AssigneeId));
        Assert.All(sample.Where(c => c.Status != ClaimStatus.Submitted), c => Assert.Equal(DemoUsers.SampleStaff, c.AssigneeId));
    }

    [Theory]
    [InlineData("SUBMITTED", ClaimStatus.Submitted)]
    [InlineData("ASSIGNED", ClaimStatus.UnderReview)]
    [InlineData("UNDER_REVIEW", ClaimStatus.UnderReview)]
    [InlineData("REOPENED", ClaimStatus.UnderReview)]
    [InlineData("AWAITING_INFO", ClaimStatus.InfoRequested)]
    [InlineData("APPROVED", ClaimStatus.Settled)]
    [InlineData("SETTLED", ClaimStatus.Settled)]
    [InlineData("CLOSED", ClaimStatus.Settled)]
    [InlineData("REJECTED", ClaimStatus.Rejected)]
    [InlineData("WITHDRAWN", ClaimStatus.Rejected)]
    public void D7_MapStatus_SourceStatus_ReturnsTheNearestStatus(string source, ClaimStatus expected)
    {
        Assert.Equal(expected, SampleDataLoader.MapStatus(source));
    }

    [Fact]
    public void D7_IdFor_SameSourceId_ReturnsTheSameGuid()
    {
        Assert.Equal(SampleDataLoader.IdFor("CLM-001077"), SampleDataLoader.IdFor("CLM-001077"));
        Assert.NotEqual(SampleDataLoader.IdFor("CLM-001077"), SampleDataLoader.IdFor("CLM-001078"));
    }

    [Fact]
    public void D7_Load_EveryClaim_StatusMatchesItsLastHistoryRow()
    {
        var mismatched = Data.Claims.Where(c => c.StatusHistory.Last().ToStatus != c.Status).ToList();

        Assert.Empty(mismatched);
    }

    [Fact]
    public void D7_Load_EveryClaim_HistoryChainsWithoutRepeatingAStatus()
    {
        var broken = Data.Claims.Where(c => c.StatusHistory.Zip(c.StatusHistory.Skip(1))
            .Any(p => p.Second.FromStatus != p.First.ToStatus || p.Second.ToStatus == p.First.ToStatus)).ToList();

        Assert.Empty(broken);
    }

    [Fact]
    public void D7_Load_EveryClaim_HasAUniqueClaimNumberAndKnownPeople()
    {
        var userIds = Data.Users.Select(u => u.Id).ToHashSet();

        Assert.Equal(Data.Claims.Count, Data.Claims.Select(c => c.ClaimNumber).Distinct().Count());
        Assert.All(Data.Claims, c => Assert.Contains(c.ClaimantId, userIds));
        Assert.All(Data.Claims, c => Assert.True(c.AssigneeId is null || userIds.Contains(c.AssigneeId.Value)));
        Assert.All(Data.Claims, c => Assert.All(c.StatusHistory, h => Assert.Contains(h.ChangedBy, userIds)));
        Assert.All(Data.Claims, c => Assert.All(c.InformationRequests, r => Assert.Contains(r.RequestedBy, userIds)));
    }

    [Fact]
    public void D7_Load_DecidedClaims_CarryADecisionAndOpenClaimsDoNot()
    {
        Assert.All(
            Data.Claims.Where(c => c.Status is ClaimStatus.Settled or ClaimStatus.Rejected),
            c => Assert.True(c.DecisionOutcome is not null && c.DecidedAt is not null));
        Assert.All(
            Data.Claims.Where(c => c.Status is ClaimStatus.Submitted or ClaimStatus.UnderReview or ClaimStatus.InfoRequested),
            c => Assert.True(c.DecisionOutcome is null && c.DecidedAt is null));
    }

    [Fact]
    public void D7_Load_Claims_RaiseNoEvents()
    {
        Assert.All(Data.Claims, c => Assert.Empty(c.DomainEvents));
    }

    [Fact]
    public void D7_Load_OpenInformationRequests_ExistOnlyWithoutAResponse()
    {
        var requests = Data.Claims.SelectMany(c => c.InformationRequests).ToList();

        Assert.Contains(requests, r => r.IsOpen);
        Assert.All(requests.Where(r => r.IsOpen), r => Assert.Null(r.Response));
        Assert.All(requests.Where(r => !r.IsOpen), r => Assert.NotNull(r.RespondedAt));
    }

    [Fact]
    public void D7_Load_ClaimedAmounts_AreInUsdAndPositive()
    {
        // The first claim is AUD 13,850.27 at 0.65 to the dollar.
        const decimal ExpectedUsd = 9002.68m;
        var first = Data.Claims.Single(c => c.ClaimNumber == "AU2510-011078");

        Assert.Equal(ExpectedUsd, first.ClaimedAmount);
        Assert.All(Data.Claims, c => Assert.True(c.ClaimedAmount > 0));
    }
}
