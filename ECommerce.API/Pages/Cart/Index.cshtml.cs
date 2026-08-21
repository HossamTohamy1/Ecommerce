
using ECommerce.Application.Features.Addresses.Queries.GetMyAddresses;
using ECommerce.Application.Features.Carts.Commands.ClearCart;
using ECommerce.Application.Features.Carts.Commands.RemoveFromCart;
using ECommerce.Application.Features.Carts.Commands.UpdateCartItem;
using ECommerce.Application.Features.Carts.Queries.GetMyCart;

namespace ECommerce.API.Pages.Cart;

public class IndexModel : RazorPageBase
{
    private readonly IMediator _mediator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public IndexModel(IMediator mediator, IStringLocalizer<SharedResource> localizer)
    {
        _mediator = mediator;
        _localizer = localizer;
    }

    public CartDto Cart { get; set; } = new();
    public bool HasAddresses { get; set; }

    public async Task OnGetAsync()
    {
        Cart = await _mediator.Send(new GetMyCartQuery(CurrentUserId));
        HasAddresses = (await _mediator.Send(new GetMyAddressesQuery(CurrentUserId))).Count > 0;
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid itemId, int quantity)
    {
        var result = await _mediator.Send(new UpdateCartItemCommand(CurrentUserId, itemId, new UpdateCartItemRequest { Quantity = quantity }));
        if (!result.Succeeded) SetError(result.Error);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid itemId)
    {
        var result = await _mediator.Send(new RemoveFromCartCommand(CurrentUserId, itemId));
        if (result.Succeeded) SetSuccess(_localizer); else SetError(result.Error);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostClearAsync()
    {
        await _mediator.Send(new ClearCartCommand(CurrentUserId));
        return RedirectToPage();
    }
}

