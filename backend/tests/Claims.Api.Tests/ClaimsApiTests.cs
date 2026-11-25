using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Api.Application.Claims;
using Claims.Api.Domain;
using Claims.Api.Infrastructure.Persistence.Seeders;
using Claims.Api.Tests.Infrastructure;

namespace Claims.Api.Tests;

/// <summary>The Must slice end to end against real PostgreSQL (Testcontainers). Test names carry the RTM requirement id.</summary>
[Collection(ApiCollection.Name)]
public class ClaimsApiTests(ClaimsApiFactory factory)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client = factory.CreateClient();

    private async Task<ClaimResponse> SubmitAsync(Guid? claimantId = null)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/claims",
            new SubmitClaimRequest(claimantId ?? DemoUsers.Alice, ClaimType.Motor, new DateOnly(2026, 11, 20), "Rear-ended at a junction", 1200m),
            Json);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ClaimResponse>(Json))!;
    }

    private async Task<HttpResponseMessage> PostAsync(Guid claimId, string action, object body) =>
        await _client.PostAsJsonAsync($"/api/claims/{claimId}/{action}", body, Json);

    private async Task<ClaimResponse> ReadAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ClaimResponse>(Json))!;
    }

    [Fact]
    public async Task R1_Claimant_reports_an_incident_and_can_track_it_R2()
    {
        var created = await SubmitAsync();

        var tracked = await ReadAsync(await _client.GetAsync($"/api/claims/{created.Id}"));

        Assert.Equal(ClaimStatus.Submitted, tracked.Status);
        Assert.Equal(DemoUsers.Alice, tracked.ClaimantId);
        Assert.Single(tracked.StatusHistory);

        var mine = await _client.GetFromJsonAsync<List<ClaimSummaryResponse>>($"/api/claims?claimantId={DemoUsers.Alice}", Json);
        Assert.Contains(mine!, c => c.Id == created.Id);
    }

    [Fact]
    public async Task R1_Invalid_report_returns_validation_problem()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/claims",
            new SubmitClaimRequest(DemoUsers.Alice, ClaimType.Motor, new DateOnly(2026, 11, 20), "", -5m),
            Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.True(problem.GetProperty("errors").TryGetProperty("Description", out _));
        Assert.True(problem.GetProperty("errors").TryGetProperty("ClaimedAmount", out _));
    }

    [Fact]
    public async Task R1_An_officer_cannot_report_a_claim()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/claims",
            new SubmitClaimRequest(DemoUsers.Carol, ClaimType.Motor, new DateOnly(2026, 11, 20), "Description", 100m),
            Json);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task R2_Unknown_claim_returns_not_found()
    {
        var response = await _client.GetAsync($"/api/claims/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task R5_Officer_sees_unassigned_claims_and_picks_one_up()
    {
        var created = await SubmitAsync();

        var queue = await _client.GetFromJsonAsync<List<ClaimSummaryResponse>>("/api/claims?unassigned=true&status=Submitted", Json);
        Assert.Contains(queue!, c => c.Id == created.Id);

        var picked = await ReadAsync(await PostAsync(created.Id, "pick-up", new PickUpClaimRequest(DemoUsers.Carol)));

        Assert.Equal(ClaimStatus.UnderReview, picked.Status);
        Assert.Equal(DemoUsers.Carol, picked.AssigneeId);

        var after = await _client.GetFromJsonAsync<List<ClaimSummaryResponse>>("/api/claims?unassigned=true", Json);
        Assert.DoesNotContain(after!, c => c.Id == created.Id);
    }

    [Fact]
    public async Task R5_Two_officers_picking_up_the_same_claim_cannot_both_succeed()
    {
        var created = await SubmitAsync();

        var responses = await Task.WhenAll(
            PostAsync(created.Id, "pick-up", new PickUpClaimRequest(DemoUsers.Carol)),
            PostAsync(created.Id, "pick-up", new PickUpClaimRequest(DemoUsers.Dave)));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task R6_And_R3_Officer_asks_and_claimant_answers()
    {
        var created = await SubmitAsync();
        await ReadAsync(await PostAsync(created.Id, "pick-up", new PickUpClaimRequest(DemoUsers.Carol)));

        var asked = await ReadAsync(await PostAsync(
            created.Id, "information-requests", new RequestInformationRequest(DemoUsers.Carol, "Please send the police report")));
        Assert.Equal(ClaimStatus.InfoRequested, asked.Status);
        Assert.Null(Assert.Single(asked.InformationRequests).Response);

        var answered = await ReadAsync(await PostAsync(
            created.Id, "information-response", new ProvideInformationRequest(DemoUsers.Alice, "Report 4411 attached")));

        Assert.Equal(ClaimStatus.UnderReview, answered.Status);
        Assert.Equal("Report 4411 attached", Assert.Single(answered.InformationRequests).Response);
    }

    [Fact]
    public async Task R3_Another_claimant_cannot_answer()
    {
        var created = await SubmitAsync();
        await ReadAsync(await PostAsync(created.Id, "pick-up", new PickUpClaimRequest(DemoUsers.Carol)));
        await ReadAsync(await PostAsync(created.Id, "information-requests", new RequestInformationRequest(DemoUsers.Carol, "Question?")));

        var response = await PostAsync(created.Id, "information-response", new ProvideInformationRequest(DemoUsers.Bob, "Not my claim"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task R7_Assigned_officer_settles_and_claimant_sees_the_decision_R4()
    {
        var created = await SubmitAsync();
        await ReadAsync(await PostAsync(created.Id, "pick-up", new PickUpClaimRequest(DemoUsers.Carol)));

        var settled = await ReadAsync(await PostAsync(created.Id, "settle", new SettleClaimRequest(DemoUsers.Carol, "Covered")));
        Assert.Equal(ClaimStatus.Settled, settled.Status);

        var seen = await ReadAsync(await _client.GetAsync($"/api/claims/{created.Id}"));
        Assert.Equal(DecisionOutcome.Settled, seen.DecisionOutcome);
        Assert.Equal("Covered", seen.DecisionReason);
        Assert.NotNull(seen.DecidedAt);
        Assert.Equal(
            [ClaimStatus.Submitted, ClaimStatus.UnderReview, ClaimStatus.Settled],
            seen.StatusHistory.Select(h => h.ToStatus));
    }

    [Fact]
    public async Task R7_Rejection_needs_a_reason()
    {
        var created = await SubmitAsync();
        await ReadAsync(await PostAsync(created.Id, "pick-up", new PickUpClaimRequest(DemoUsers.Carol)));

        var response = await PostAsync(created.Id, "reject", new RejectClaimRequest(DemoUsers.Carol, ""));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task R7_Rejection_with_a_reason_is_final()
    {
        var created = await SubmitAsync();
        await ReadAsync(await PostAsync(created.Id, "pick-up", new PickUpClaimRequest(DemoUsers.Carol)));

        var rejected = await ReadAsync(await PostAsync(created.Id, "reject", new RejectClaimRequest(DemoUsers.Carol, "Policy had lapsed")));
        Assert.Equal(ClaimStatus.Rejected, rejected.Status);

        var again = await PostAsync(created.Id, "settle", new SettleClaimRequest(DemoUsers.Carol, null));
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
    }

    [Fact]
    public async Task R7_An_officer_who_does_not_own_the_claim_cannot_decide()
    {
        var created = await SubmitAsync();
        await ReadAsync(await PostAsync(created.Id, "pick-up", new PickUpClaimRequest(DemoUsers.Carol)));

        var response = await PostAsync(created.Id, "settle", new SettleClaimRequest(DemoUsers.Dave, null));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task R7_Deciding_before_pick_up_returns_conflict()
    {
        var created = await SubmitAsync();

        var response = await PostAsync(created.Id, "settle", new SettleClaimRequest(DemoUsers.Carol, null));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
