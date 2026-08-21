using FluentValidation;

namespace ECommerce.Application.Features.Carts.Commands.AddToCart;

public class AddToCartCommandValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.ProductId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
            RuleFor(x => x.Request.Quantity).GreaterThan(0).WithMessage(localizer["Validation.Range"].Value);
        });
    }
}
