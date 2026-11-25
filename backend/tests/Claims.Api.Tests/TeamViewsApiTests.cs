using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Api.Application.Claims;
using Claims.Api.Application.Exposure;
using Claims.Api.Application.Workload;
using Claims.Api.Domain;
using Claims.Api.Infrastructure.Persistence.Seeders;
using Claims.Api.Tests.Infrastructure;

namespace Claims.Api.Tests;

/// <summary>
/// Integration tests for RFC-0003: time in status on the claim list, team workload (R8) and claimed exposure (D7).
/// Other tests share the database, so figures are compared with a baseline read in the arrange step.
/// Names follow R{requirement}_ or D{design question}_ (ADR-0013, ADR-0020).
/// </summary>
[Collection(ApiCollection.Name)]
public class TeamViewsApiTests(ClaimsApiFactory factory)
{
    private const string Description = "description";
    private const string Reason = "reason";
    private const decimal Amount = 1000m;

    private static readonly DateOnly IncidentDate = new(2026, 11, 20);

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client = factory.CreateClient();

    // --- Arrange and act helpers ---------------------------------------------------------------

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, HttpStatusCode expected = HttpStatusCode.OK)
    {
        Assert.Equal(expected, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    private Task<HttpResponseMessage> PostAsync(string path, object body) => _client.PostAsJsonAsync(path, body, Json);

    private async Task<ClaimResponse> SubmittedAsync(decimal amount = Amount) =>
        await ReadAsync<ClaimResponse>(
            await PostAsync("/api/claims", new SubmitClaimRequest(DemoUsers.Alice, ClaimType.Motor, IncidentDate, Description, amount)),
            HttpStatusCode.Created);

    private async Task<ClaimResponse> PickedUpAsync(Guid claimId, Guid officerId) =>
        await ReadAsync<ClaimResponse>(await PostAsync($"/api/claims/{claimId}/pick-up", new PickUpClaimRequest(officerId)));

    private async Task<ClaimResponse> UnderReviewAsync(Guid officerId, decimal amount = Amount) =>
        await PickedUpAsync((await SubmittedAsync(amount)).Id, officerId);

    private Task<HttpResponseMessage> SettleRequestAsync(Guid claimId, Guid officerId) =>
        PostAsync($"/api/claims/{claimId}/settle", new SettleClaimRequest(officerId, Reason));

    private async Task<List<ClaimSummaryResponse>> ListAsync(string query) =>
        (await _client.GetFromJsonAsync<List<ClaimSummaryResponse>>($"/api/claims{query}", Json))!;

    private async Task<WorkloadResponse> WorkloadAsync(Guid officerId) =>
        await ReadAsync<WorkloadResponse>(await _client.GetAsync($"/api/workload?officerId={officerId}"));

    private async Task<ExposureResponse> ExposureAsync() =>
        await ReadAsync<ExposureResponse>(await _client.GetAsync($"/api/exposure?officerId={DemoUsers.Carol}"));

    private static OfficerWorkloadResponse For(WorkloadResponse workload, Guid officerId) =>
        workload.Officers.Single(o => o.OfficerId == officerId);

    private static decimal SubmittedAmount(ExposureResponse exposure) =>
        exposure.ByStatus.Single(s => s.Status == ClaimStatus.Submitted).ClaimedAmount;

    // --- D6: time in current status on the claim list ------------------------------------------

    [Fact]
    public async Task R5_ListClaims_PickedUp_StatusChangedAtIsThePickUp()
    {
        var claim = await UnderReviewAsync(DemoUsers.Dave);
        var pickUp = (await _client.GetFromJsonAsync<ClaimResponse>($"/api/claims/{claim.Id}", Json))!.StatusHistory[^1];

        var row = (await ListAsync($"?assigneeId={DemoUsers.Dave}")).Single(c => c.Id == claim.Id);

        Assert.Equal(pickUp.ChangedAt, row.StatusChangedAt);
    }

    [Fact]
    public async Task R5_ListClaims_SortByStatusChangedAt_PutsTheLongestInStatusFirst()
    {
        var first = await SubmittedAsync();
        var second = await SubmittedAsync();
        await PickedUpAsync(first.Id, DemoUsers.Dave);

        var claims = await ListAsync($"?claimantId={DemoUsers.Alice}&sort=StatusChangedAt");

        Assert.True(claims.FindIndex(c => c.Id == second.Id) < claims.FindIndex(c => c.Id == first.Id));
    }

    [Fact]
    public async Task R5_ListClaims_UnknownSort_Returns400()
    {
        var response = await _client.GetAsync("/api/claims?sort=Nonsense");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- R8: team workload and performance ------------------------------------------------------

    [Fact]
    public async Task R8_Workload_Claimant_Returns403()
    {
        var response = await _client.GetAsync($"/api/workload?officerId={DemoUsers.Alice}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task R8_Workload_NoOfficerId_Returns400()
    {
        var response = await _client.GetAsync("/api/workload");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task R8_Workload_Officer_ListsEveryOfficerOnTheTeam()
    {
        var workload = await WorkloadAsync(DemoUsers.Carol);

        Assert.Equal(DemoUsers.Team, workload.TeamId);
        Assert.Equal([DemoUsers.Carol, DemoUsers.Dave], workload.Officers.Select(o => o.OfficerId));
    }

    [Fact]
    public async Task R8_Workload_ClaimPickedUp_CountsUnderReviewForTheOfficer()
    {
        var before = For(await WorkloadAsync(DemoUsers.Carol), DemoUsers.Dave);
        await UnderReviewAsync(DemoUsers.Dave);

        var after = For(await WorkloadAsync(DemoUsers.Carol), DemoUsers.Dave);

        Assert.Equal(before.UnderReview + 1, after.UnderReview);
    }

    [Fact]
    public async Task R8_Workload_ClaimSubmitted_CountsAsUnassigned()
    {
        var before = await WorkloadAsync(DemoUsers.Carol);
        await SubmittedAsync();

        var after = await WorkloadAsync(DemoUsers.Carol);

        Assert.Equal(before.Unassigned.Count + 1, after.Unassigned.Count);
    }

    [Fact]
    public async Task R8_Workload_ClaimSettled_CountsADecisionInThePeriod()
    {
        var claim = await UnderReviewAsync(DemoUsers.Carol);
        var before = For(await WorkloadAsync(DemoUsers.Carol), DemoUsers.Carol);
        Assert.Equal(HttpStatusCode.OK, (await SettleRequestAsync(claim.Id, DemoUsers.Carol)).StatusCode);

        var after = For(await WorkloadAsync(DemoUsers.Carol), DemoUsers.Carol);

        Assert.Equal(
            (before.DecidedInPeriod + 1, before.SettledInPeriod + 1, before.UnderReview - 1),
            (after.DecidedInPeriod, after.SettledInPeriod, after.UnderReview));
        Assert.NotNull(after.AverageDaysToDecision);
    }

    // --- D7: claimed exposure -------------------------------------------------------------------

    [Fact]
    public async Task D7_Exposure_Claimant_Returns403()
    {
        var response = await _client.GetAsync($"/api/exposure?officerId={DemoUsers.Alice}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task D7_Exposure_ClaimSubmitted_AddsItsClaimedAmount()
    {
        var before = await ExposureAsync();
        await SubmittedAsync(Amount);

        var after = await ExposureAsync();

        Assert.Equal(
            (before.OpenClaims + 1, before.ClaimedExposure + Amount, SubmittedAmount(before) + Amount),
            (after.OpenClaims, after.ClaimedExposure, SubmittedAmount(after)));
    }

    [Fact]
    public async Task D7_Exposure_ClaimSettled_RemovesItsClaimedAmount()
    {
        var claim = await UnderReviewAsync(DemoUsers.Carol, Amount);
        var before = await ExposureAsync();
        Assert.Equal(HttpStatusCode.OK, (await SettleRequestAsync(claim.Id, DemoUsers.Carol)).StatusCode);

        var after = await ExposureAsync();

        Assert.Equal(before.ClaimedExposure - Amount, after.ClaimedExposure);
    }

    [Fact]
    public async Task D7_Exposure_Breakdowns_AddUpToTheTotal()
    {
        await SubmittedAsync();

        var exposure = await ExposureAsync();

        Assert.Equal(
            (exposure.ClaimedExposure, exposure.ClaimedExposure, exposure.ClaimedExposure),
            (exposure.ByStatus.Sum(s => s.ClaimedAmount), exposure.ByType.Sum(t => t.ClaimedAmount), exposure.ByAge.Sum(a => a.ClaimedAmount)));
    }
}
