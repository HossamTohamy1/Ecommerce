using FluentValidation;

namespace ECommerce.Application.Features.Products.Commands.AddProductVariant;

public class AddProductVariantCommandValidator : AbstractValidator<AddProductVariantCommand>
{
    public AddProductVariantCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.SKU).RequiredField(localizer).MaximumLength(50).WithMessage(localizer["Validation.MaxLength"].Value);
            RuleFor(x => x.Request.Price).GreaterThan(0).When(x => x.Request.Price.HasValue).WithMessage(localizer["Validation.GreaterThanZero"].Value);
            RuleFor(x => x.Request.StockQuantity).GreaterThanOrEqualTo(0).WithMessage(localizer["Validation.GreaterThanOrEqualToZero"].Value);
        });
    }
}
