using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Api.Application.Claims;
using Claims.Api.Domain;
using Claims.Api.Infrastructure.Persistence;
using Claims.Api.Infrastructure.Persistence.Seeders;
using Claims.Api.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Api.Tests;

/// <summary>
/// Integration tests: the Must slice end to end against real PostgreSQL (Testcontainers). Requires Docker.
/// Names follow R{requirement}_{Action}_{Scenario}_{ExpectedBehavior}. Arrange steps go through helpers, so each
/// test has one act.
/// </summary>
[Collection(ApiCollection.Name)]
public class ClaimsApiTests(ClaimsApiFactory factory)
{
    private const string Description = "description";
    private const string Question = "question";
    private const string Answer = "answer";
    private const string Reason = "reason";
    private const decimal Amount = 1m;

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

    private static int IndexOf(List<ClaimSummaryResponse> claims, Guid id) => claims.FindIndex(c => c.Id == id);

    private Task<HttpResponseMessage> PostAsync(string path, object body) => _client.PostAsJsonAsync(path, body, Json);

    private Task<HttpResponseMessage> SubmitRequestAsync(Guid claimantId, string description = Description, decimal amount = Amount) =>
        PostAsync("/api/claims", new SubmitClaimRequest(claimantId, ClaimType.Motor, IncidentDate, description, amount));

    private Task<HttpResponseMessage> PickUpRequestAsync(Guid claimId, Guid officerId) =>
        PostAsync($"/api/claims/{claimId}/pick-up", new PickUpClaimRequest(officerId));

    private Task<HttpResponseMessage> AskRequestAsync(Guid claimId, Guid officerId) =>
        PostAsync($"/api/claims/{claimId}/information-requests", new RequestInformationRequest(officerId, Question));

    private Task<HttpResponseMessage> AnswerRequestAsync(Guid claimId, Guid claimantId) =>
        PostAsync($"/api/claims/{claimId}/information-response", new ProvideInformationRequest(claimantId, Answer));

    private Task<HttpResponseMessage> SettleRequestAsync(Guid claimId, Guid officerId) =>
        PostAsync($"/api/claims/{claimId}/settle", new SettleClaimRequest(officerId, Reason));

    private Task<HttpResponseMessage> RejectRequestAsync(Guid claimId, Guid officerId, string reason = Reason) =>
        PostAsync($"/api/claims/{claimId}/reject", new RejectClaimRequest(officerId, reason));

    private async Task<ClaimResponse> SubmittedAsync(Guid? claimantId = null) =>
        await ReadAsync<ClaimResponse>(await SubmitRequestAsync(claimantId ?? DemoUsers.Alice), HttpStatusCode.Created);

    private async Task<ClaimResponse> UnderReviewAsync()
    {
        var claim = await SubmittedAsync();
        return await ReadAsync<ClaimResponse>(await PickUpRequestAsync(claim.Id, DemoUsers.Carol));
    }

    private async Task<ClaimResponse> InfoRequestedAsync()
    {
        var claim = await UnderReviewAsync();
        return await ReadAsync<ClaimResponse>(await AskRequestAsync(claim.Id, DemoUsers.Carol));
    }

    private async Task<ClaimResponse> SettledAsync()
    {
        var claim = await UnderReviewAsync();
        return await ReadAsync<ClaimResponse>(await SettleRequestAsync(claim.Id, DemoUsers.Carol));
    }

    private async Task<List<ClaimSummaryResponse>> ListAsync(string query = "") =>
        (await _client.GetFromJsonAsync<List<ClaimSummaryResponse>>($"/api/claims{query}", Json))!;

    // --- R1: report an incident ----------------------------------------------------------------

    [Fact]
    public async Task R1_Submit_ValidReport_ReturnsCreatedSubmittedClaim()
    {
        var response = await SubmitRequestAsync(DemoUsers.Alice);

        var claim = await ReadAsync<ClaimResponse>(response, HttpStatusCode.Created);
        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.Equal(DemoUsers.Alice, claim.ClaimantId);
    }

    [Fact]
    public async Task R1_Submit_InvalidReport_ReturnsValidationProblem()
    {
        var response = await SubmitRequestAsync(DemoUsers.Alice, description: string.Empty, amount: -1m);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        Assert.True(problem.GetProperty("errors").TryGetProperty("Description", out _));
        Assert.True(problem.GetProperty("errors").TryGetProperty("ClaimedAmount", out _));
    }

    [Fact]
    public async Task R1_Submit_OfficerAsClaimant_ReturnsForbidden()
    {
        var response = await SubmitRequestAsync(DemoUsers.Carol);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // --- R2: track a claim ---------------------------------------------------------------------

    [Fact]
    public async Task R2_Get_SubmittedClaim_ReturnsStatusAndHistory()
    {
        var created = await SubmittedAsync();

        var response = await _client.GetAsync($"/api/claims/{created.Id}");

        var claim = await ReadAsync<ClaimResponse>(response);
        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.Single(claim.StatusHistory);
    }

    [Fact]
    public async Task R2_Get_UnknownClaim_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/claims/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task R2_List_ByClaimant_ReturnsOnlyThatClaimantsClaims()
    {
        var alices = await SubmittedAsync(DemoUsers.Alice);
        var bobs = await SubmittedAsync(DemoUsers.Bob);

        var claims = await ListAsync($"?claimantId={DemoUsers.Bob}");

        Assert.Contains(claims, c => c.Id == bobs.Id);
        Assert.DoesNotContain(claims, c => c.Id == alices.Id);
    }

    [Fact]
    public async Task R2_List_NoFilter_ReturnsOldestFirst()
    {
        var first = await SubmittedAsync();
        var second = await SubmittedAsync();

        var claims = await ListAsync();

        Assert.True(IndexOf(claims, first.Id) < IndexOf(claims, second.Id));
    }

    // --- R4: receive a decision ----------------------------------------------------------------

    [Fact]
    public async Task R4_Get_SettledClaim_ShowsTheDecisionToTheClaimant()
    {
        var settled = await SettledAsync();

        var response = await _client.GetAsync($"/api/claims/{settled.Id}");

        var claim = await ReadAsync<ClaimResponse>(response);
        Assert.Equal(DecisionOutcome.Settled, claim.DecisionOutcome);
        Assert.Equal(Reason, claim.DecisionReason);
        Assert.NotNull(claim.DecidedAt);
    }

    // --- R5: pick up an incoming claim ---------------------------------------------------------

    [Fact]
    public async Task R5_PickUp_SubmittedClaim_AssignsTheOfficerAndStartsReview()
    {
        var created = await SubmittedAsync();

        var response = await PickUpRequestAsync(created.Id, DemoUsers.Carol);

        var claim = await ReadAsync<ClaimResponse>(response);
        Assert.Equal(ClaimStatus.UnderReview, claim.Status);
        Assert.Equal(DemoUsers.Carol, claim.AssigneeId);
    }

    [Fact]
    public async Task R5_PickUp_TwoOfficersAtOnce_OnlyOneSucceeds()
    {
        var created = await SubmittedAsync();

        var responses = await Task.WhenAll(
            PickUpRequestAsync(created.Id, DemoUsers.Carol),
            PickUpRequestAsync(created.Id, DemoUsers.Dave));

        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task R5_PickUp_UnknownClaim_ReturnsNotFound()
    {
        var response = await PickUpRequestAsync(Guid.NewGuid(), DemoUsers.Carol);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task R5_PickUp_ClaimantAsOfficer_ReturnsForbidden()
    {
        var created = await SubmittedAsync();

        var response = await PickUpRequestAsync(created.Id, DemoUsers.Alice);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task R5_List_Unassigned_ReturnsOnlyClaimsNotYetPickedUp()
    {
        var waiting = await SubmittedAsync();
        var taken = await UnderReviewAsync();

        var claims = await ListAsync("?unassigned=true");

        Assert.Contains(claims, c => c.Id == waiting.Id);
        Assert.DoesNotContain(claims, c => c.Id == taken.Id);
    }

    [Fact]
    public async Task R5_List_Unassigned_RejectedClaimWithNoOfficer_IsNotListed()
    {
        var claim = await UnderReviewAsync();
        await ReadAsync<ClaimResponse>(await RejectRequestAsync(claim.Id, DemoUsers.Carol));

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();
        await db.Claims.Where(c => c.Id == claim.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.AssigneeId, (Guid?)null));

        var claims = await ListAsync("?unassigned=true");

        Assert.DoesNotContain(claims, c => c.Id == claim.Id);
        Assert.All(claims, c => Assert.Equal(ClaimStatus.Submitted, c.Status));
    }

    [Fact]
    public async Task R5_List_ByAssignee_ReturnsOnlyThatOfficersClaims()
    {
        var waiting = await SubmittedAsync();
        var carols = await UnderReviewAsync();

        var claims = await ListAsync($"?assigneeId={DemoUsers.Carol}");

        Assert.Contains(claims, c => c.Id == carols.Id);
        Assert.DoesNotContain(claims, c => c.Id == waiting.Id);
    }

    [Fact]
    public async Task R5_List_ByStatus_ReturnsOnlyClaimsInThatStatus()
    {
        var waiting = await SubmittedAsync();
        var underReview = await UnderReviewAsync();

        var claims = await ListAsync("?status=UnderReview");

        Assert.Contains(claims, c => c.Id == underReview.Id);
        Assert.DoesNotContain(claims, c => c.Id == waiting.Id);
    }

    // --- R6: review and ask for information ----------------------------------------------------

    [Fact]
    public async Task R6_RequestInformation_AssignedOfficer_MovesToInfoRequested()
    {
        var claim = await UnderReviewAsync();

        var response = await AskRequestAsync(claim.Id, DemoUsers.Carol);

        var asked = await ReadAsync<ClaimResponse>(response);
        Assert.Equal(ClaimStatus.InfoRequested, asked.Status);
        Assert.Null(Assert.Single(asked.InformationRequests).Response);
    }

    [Fact]
    public async Task R6_RequestInformation_WhileARequestIsOpen_ReturnsConflict()
    {
        var claim = await InfoRequestedAsync();

        var response = await AskRequestAsync(claim.Id, DemoUsers.Carol);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // --- R3: provide additional information ----------------------------------------------------

    [Fact]
    public async Task R3_ProvideInformation_OpenRequest_StoresTheAnswerAndReturnsToReview()
    {
        var claim = await InfoRequestedAsync();

        var response = await AnswerRequestAsync(claim.Id, DemoUsers.Alice);

        var answered = await ReadAsync<ClaimResponse>(response);
        Assert.Equal(ClaimStatus.UnderReview, answered.Status);
        Assert.Equal(Answer, Assert.Single(answered.InformationRequests).Response);
    }

    [Fact]
    public async Task R3_ProvideInformation_OtherClaimant_ReturnsForbidden()
    {
        var claim = await InfoRequestedAsync();

        var response = await AnswerRequestAsync(claim.Id, DemoUsers.Bob);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task R3_ProvideInformation_SecondRound_KeepsBothAnswers()
    {
        var claim = await InfoRequestedAsync();
        await ReadAsync<ClaimResponse>(await AnswerRequestAsync(claim.Id, DemoUsers.Alice));
        await ReadAsync<ClaimResponse>(await AskRequestAsync(claim.Id, DemoUsers.Carol));

        var response = await AnswerRequestAsync(claim.Id, DemoUsers.Alice);

        var answered = await ReadAsync<ClaimResponse>(response);
        Assert.Equal(2, answered.InformationRequests.Count);
        Assert.All(answered.InformationRequests, r => Assert.Equal(Answer, r.Response));
    }

    // --- R7: settle or reject ------------------------------------------------------------------

    [Fact]
    public async Task R7_Settle_AssignedOfficer_SettlesTheClaim()
    {
        var claim = await UnderReviewAsync();

        var response = await SettleRequestAsync(claim.Id, DemoUsers.Carol);

        var settled = await ReadAsync<ClaimResponse>(response);
        Assert.Equal(ClaimStatus.Settled, settled.Status);
        Assert.Equal(DecisionOutcome.Settled, settled.DecisionOutcome);
    }

    [Fact]
    public async Task R7_Reject_AssignedOfficerWithReason_RejectsTheClaim()
    {
        var claim = await UnderReviewAsync();

        var response = await RejectRequestAsync(claim.Id, DemoUsers.Carol);

        var rejected = await ReadAsync<ClaimResponse>(response);
        Assert.Equal(ClaimStatus.Rejected, rejected.Status);
        Assert.Equal(Reason, rejected.DecisionReason);
    }

    [Fact]
    public async Task R7_Reject_EmptyReason_ReturnsValidationProblem()
    {
        var claim = await UnderReviewAsync();

        var response = await RejectRequestAsync(claim.Id, DemoUsers.Carol, reason: string.Empty);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task R7_Settle_AlreadyRejectedClaim_ReturnsConflict()
    {
        var claim = await UnderReviewAsync();
        await ReadAsync<ClaimResponse>(await RejectRequestAsync(claim.Id, DemoUsers.Carol));

        var response = await SettleRequestAsync(claim.Id, DemoUsers.Carol);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task R7_Settle_OtherOfficer_ReturnsForbidden()
    {
        var claim = await UnderReviewAsync();

        var response = await SettleRequestAsync(claim.Id, DemoUsers.Dave);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task R7_Settle_ClaimNotPickedUp_ReturnsConflict()
    {
        var claim = await SubmittedAsync();

        var response = await SettleRequestAsync(claim.Id, DemoUsers.Carol);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
