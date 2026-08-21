using FluentValidation;

namespace ECommerce.Application.Features.Auth.Commands.Logout;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.RefreshToken).RequiredField(localizer);
        });
    }
}
