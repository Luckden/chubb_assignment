using Claims.Api.Application.Claims;
using Claims.Api.Domain;
using FluentValidation.TestHelper;

namespace Claims.Api.Tests.Application;

/// <summary>
/// Request validation rules. Pure unit tests: no database, no Docker.
/// Names follow R{requirement}_{Validator}_{Scenario}_{ExpectedBehavior}.
/// </summary>
public class ClaimValidatorTests
{
    private const string Text = "text";
    private const decimal Amount = 1m;

    private static readonly Guid Someone = Guid.NewGuid();
    private static readonly DateOnly IncidentDate = new(2026, 11, 20);
    private static readonly string TooLong = new('x', 2001);

    public static TheoryData<decimal> InvalidAmounts => new() { 0m, -1m, 10.123m };

    public static TheoryData<string> BlankTexts => new() { string.Empty, " " };

    private static SubmitClaimRequest Submit(
        Guid? claimantId = null,
        ClaimType type = ClaimType.Motor,
        string description = Text,
        decimal amount = Amount) =>
        new(claimantId ?? Someone, type, IncidentDate, description, amount);

    // R1: report an incident

    [Fact]
    public void R1_SubmitValidator_CompleteReport_HasNoErrors()
    {
        var result = new SubmitClaimRequestValidator().TestValidate(Submit());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(InvalidAmounts))]
    public void R1_SubmitValidator_AmountNotPositiveOrTooPrecise_HasAnError(decimal amount)
    {
        var result = new SubmitClaimRequestValidator().TestValidate(Submit(amount: amount));

        result.ShouldHaveValidationErrorFor(r => r.ClaimedAmount);
    }

    [Fact]
    public void R1_SubmitValidator_AmountWithTwoDecimals_HasNoErrors()
    {
        var result = new SubmitClaimRequestValidator().TestValidate(Submit(amount: 1.50m));

        result.ShouldNotHaveValidationErrorFor(r => r.ClaimedAmount);
    }

    [Theory]
    [MemberData(nameof(BlankTexts))]
    public void R1_SubmitValidator_BlankDescription_HasAnError(string description)
    {
        var result = new SubmitClaimRequestValidator().TestValidate(Submit(description: description));

        result.ShouldHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void R1_SubmitValidator_TooLongDescription_HasAnError()
    {
        var result = new SubmitClaimRequestValidator().TestValidate(Submit(description: TooLong));

        result.ShouldHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void R1_SubmitValidator_UnknownClaimType_HasAnError()
    {
        var result = new SubmitClaimRequestValidator().TestValidate(Submit(type: (ClaimType)99));

        result.ShouldHaveValidationErrorFor(r => r.Type);
    }

    [Fact]
    public void R1_SubmitValidator_EmptyClaimantId_HasAnError()
    {
        var result = new SubmitClaimRequestValidator().TestValidate(Submit(claimantId: Guid.Empty));

        result.ShouldHaveValidationErrorFor(r => r.ClaimantId);
    }

    // R5: pick up

    [Fact]
    public void R5_PickUpValidator_EmptyOfficerId_HasAnError()
    {
        var result = new PickUpClaimRequestValidator().TestValidate(new PickUpClaimRequest(Guid.Empty));

        result.ShouldHaveValidationErrorFor(r => r.OfficerId);
    }

    // R6: ask for information

    [Theory]
    [MemberData(nameof(BlankTexts))]
    public void R6_RequestInformationValidator_BlankQuestion_HasAnError(string question)
    {
        var result = new RequestInformationRequestValidator().TestValidate(new RequestInformationRequest(Someone, question));

        result.ShouldHaveValidationErrorFor(r => r.Question);
    }

    // R3: provide information

    [Theory]
    [MemberData(nameof(BlankTexts))]
    public void R3_ProvideInformationValidator_BlankResponse_HasAnError(string response)
    {
        var result = new ProvideInformationRequestValidator().TestValidate(new ProvideInformationRequest(Someone, response));

        result.ShouldHaveValidationErrorFor(r => r.Response);
    }

    // R7: settle or reject

    [Fact]
    public void R7_SettleValidator_NoReason_HasNoErrors()
    {
        var result = new SettleClaimRequestValidator().TestValidate(new SettleClaimRequest(Someone, null));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void R7_SettleValidator_TooLongReason_HasAnError()
    {
        var result = new SettleClaimRequestValidator().TestValidate(new SettleClaimRequest(Someone, TooLong));

        result.ShouldHaveValidationErrorFor(r => r.Reason);
    }

    [Theory]
    [MemberData(nameof(BlankTexts))]
    public void R7_RejectValidator_BlankReason_HasAnError(string reason)
    {
        var result = new RejectClaimRequestValidator().TestValidate(new RejectClaimRequest(Someone, reason));

        result.ShouldHaveValidationErrorFor(r => r.Reason);
    }
}
