using FluentValidation;

namespace ECommerce.Application.Features.Wishlists.Commands.RemoveFromWishlist;

public class RemoveFromWishlistCommandValidator : AbstractValidator<RemoveFromWishlistCommand>
{
    public RemoveFromWishlistCommandValidator(IStringLocalizer<SharedResource> localizer)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
        RuleFor(x => x.ProductId).NotEmpty().WithMessage(localizer["Validation.Required"].Value);
    }
}
