using FluentValidation;

namespace ECommerce.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.FullName)
                .RequiredField(localizer)
                .LengthBetween(3, 100, localizer);

            RuleFor(x => x.Request.Email)
                .RequiredField(localizer)
                .ValidEmail(localizer);

            RuleFor(x => x.Request.Password)
                .RequiredField(localizer)
                .MinimumLength(8).WithMessage(localizer["Validation.MinLength"].Value);

            RuleFor(x => x.Request.ConfirmPassword)
                .Equal(x => x.Request.Password).WithMessage(localizer["Validation.PasswordMismatch"].Value);
        });
    }
}

