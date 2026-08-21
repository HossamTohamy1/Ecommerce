using FluentValidation;

namespace ECommerce.Application.Features.Discounts.Commands.UpdateDiscount;

public class UpdateDiscountCommandValidator : AbstractValidator<UpdateDiscountCommand>
{
    public UpdateDiscountCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Name).RequiredField(localizer);
            RuleFor(x => x.Request.Value).GreaterThan(0).WithMessage(localizer["Discount.ValueMustBePositive"].Value);
        });
    }
}
