using FluentValidation;

namespace ECommerce.Application.Features.Products.Commands.UpdateProductVariant;

public class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
{
    public UpdateProductVariantCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.VariantId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.Request).NotNull().WithMessage(localizer["Validation.Required"].Value);
        When(x => x.Request != null, () =>
        {
            RuleFor(x => x.Request.SKU).RequiredField(localizer).MaximumLength(50).WithMessage(localizer["Validation.MaxLength"].Value);
            RuleFor(x => x.Request.Price).GreaterThan(0).WithMessage(localizer["Validation.Range"].Value);
        });
    }
}
