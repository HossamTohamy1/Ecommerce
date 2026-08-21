using FluentValidation;

namespace ECommerce.Application.Features.Wishlists.Commands.AddToWishlist;

public class AddToWishlistCommandValidator : AbstractValidator<AddToWishlistCommand>
{
    public AddToWishlistCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.ProductId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
    }
}
