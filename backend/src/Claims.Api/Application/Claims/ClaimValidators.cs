using FluentValidation;

namespace Claims.Api.Application.Claims;

public sealed class SubmitClaimRequestValidator : AbstractValidator<SubmitClaimRequest>
{
    public SubmitClaimRequestValidator()
    {
        RuleFor(r => r.ClaimantId).NotEmpty();
        RuleFor(r => r.Type).IsInEnum();
        RuleFor(r => r.IncidentDate).NotEmpty();
        RuleFor(r => r.Description).NotEmpty().MaximumLength(2000);
        RuleFor(r => r.ClaimedAmount).GreaterThan(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
    }
}

public sealed class PickUpClaimRequestValidator : AbstractValidator<PickUpClaimRequest>
{
    public PickUpClaimRequestValidator() => RuleFor(r => r.OfficerId).NotEmpty();
}

public sealed class RequestInformationRequestValidator : AbstractValidator<RequestInformationRequest>
{
    public RequestInformationRequestValidator()
    {
        RuleFor(r => r.OfficerId).NotEmpty();
        RuleFor(r => r.Question).NotEmpty().MaximumLength(2000);
    }
}

public sealed class ProvideInformationRequestValidator : AbstractValidator<ProvideInformationRequest>
{
    public ProvideInformationRequestValidator()
    {
        RuleFor(r => r.ClaimantId).NotEmpty();
        RuleFor(r => r.Response).NotEmpty().MaximumLength(2000);
    }
}

public sealed class SettleClaimRequestValidator : AbstractValidator<SettleClaimRequest>
{
    public SettleClaimRequestValidator()
    {
        RuleFor(r => r.OfficerId).NotEmpty();
        RuleFor(r => r.Reason).MaximumLength(2000);
    }
}

public sealed class RejectClaimRequestValidator : AbstractValidator<RejectClaimRequest>
{
    public RejectClaimRequestValidator()
    {
        RuleFor(r => r.OfficerId).NotEmpty();
        RuleFor(r => r.Reason).NotEmpty().MaximumLength(2000);
    }
}
