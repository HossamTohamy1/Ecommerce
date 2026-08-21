
using ECommerce.Application.Features.Carts.Commands.AddToCart;
using ECommerce.Application.Features.Wishlists.Commands.RemoveFromWishlist;
using ECommerce.Application.Features.Wishlists.Queries.GetMyWishlist;

namespace ECommerce.API.Pages.Wishlist;

public class IndexModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    public WishlistDto Wishlist { get; set; } = new();

    public async Task OnGetAsync()
    {
        Wishlist = await _mediator.Send(new GetMyWishlistQuery(CurrentUserId));
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid productId)
    {
        var result = await _mediator.Send(new RemoveFromWishlistCommand(CurrentUserId, productId));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddToCartAsync(Guid productId)
    {
        var result = await _mediator.Send(new AddToCartCommand(CurrentUserId, new AddCartItemRequest { ProductId = productId, Quantity = 1 }));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage();
    }
}


