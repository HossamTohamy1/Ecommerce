using FluentValidation;

namespace ECommerce.Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.Name).RequiredField(localizer).LengthBetween(2, 200, localizer);
            RuleFor(x => x.Request.SKU).RequiredField(localizer).MaximumLength(50).WithMessage(localizer["Validation.MaxLength"].Value);
            RuleFor(x => x.Request.Price).GreaterThan(0).WithMessage(localizer["Validation.GreaterThanZero"].Value);
            RuleFor(x => x.Request.CompareAtPrice)
                .GreaterThan(x => x.Request.Price)
                .When(x => x.Request.CompareAtPrice.HasValue)
                .WithMessage(localizer["Product.CompareAtPriceMustBeGreater"].Value);
            RuleFor(x => x.Request.StockQuantity).GreaterThanOrEqualTo(0).WithMessage(localizer["Validation.GreaterThanOrEqualToZero"].Value);
            RuleFor(x => x.Request.CategoryId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        });
    }
}
