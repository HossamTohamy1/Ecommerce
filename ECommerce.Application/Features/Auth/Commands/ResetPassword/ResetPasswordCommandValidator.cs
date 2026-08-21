using FluentValidation;

namespace ECommerce.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Email).RequiredField(localizer).EmailAddress().WithMessage(localizer["Validation.Email"].Value);
            RuleFor(x => x.Request.Token).RequiredField(localizer);
            RuleFor(x => x.Request.NewPassword).RequiredField(localizer).MinimumLength(6).WithMessage(localizer["Validation.MinLength"].Value);
        });
    }
}
