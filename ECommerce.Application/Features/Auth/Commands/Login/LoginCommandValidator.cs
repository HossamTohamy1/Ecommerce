using FluentValidation;

namespace ECommerce.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Email).RequiredField(localizer).EmailAddress().WithMessage(localizer["Validation.Email"].Value);
            RuleFor(x => x.Request.Password).RequiredField(localizer);
        });
    }
}
