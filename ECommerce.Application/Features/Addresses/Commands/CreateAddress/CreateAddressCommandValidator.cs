using FluentValidation;

namespace ECommerce.Application.Features.Addresses.Commands.CreateAddress;

public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
{
    public CreateAddressCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.FullName).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
            RuleFor(x => x.Request.Phone).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
            RuleFor(x => x.Request.Street).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
            RuleFor(x => x.Request.City).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
            RuleFor(x => x.Request.Governorate).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        });
    }
}
