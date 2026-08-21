using FluentValidation;

namespace ECommerce.Application.Features.Auth.Commands.VerifyTwoFactor;

public class VerifyTwoFactorCommandValidator : AbstractValidator<VerifyTwoFactorCommand>
{
    public VerifyTwoFactorCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.ChallengeId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
            RuleFor(x => x.Request.Code).RequiredField(localizer);
        });
    }
}
