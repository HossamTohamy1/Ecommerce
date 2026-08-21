using FluentValidation;

namespace ECommerce.Application.Features.Auth.Commands.ExternalLogin;

public class ExternalLoginCommandValidator : AbstractValidator<ExternalLoginCommand>
{
    public ExternalLoginCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Email).RequiredField(localizer).EmailAddress().WithMessage(localizer["Validation.Email"].Value);
            RuleFor(x => x.Request.Provider).RequiredField(localizer);
            RuleFor(x => x.Request.ProviderKey).RequiredField(localizer);
        });
    }
}
