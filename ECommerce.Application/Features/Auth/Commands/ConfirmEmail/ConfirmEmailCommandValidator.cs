using FluentValidation;

namespace ECommerce.Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
            RuleFor(x => x.Request.Token).RequiredField(localizer);
        });
    }
}
