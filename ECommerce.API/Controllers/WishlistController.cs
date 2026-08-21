
using ECommerce.Application.Features.Wishlists.Commands.AddToWishlist;
using ECommerce.Application.Features.Wishlists.Commands.RemoveFromWishlist;
using ECommerce.Application.Features.Wishlists.Queries.GetMyWishlist;

namespace ECommerce.API.Controllers;

[Route("api/wishlist")]
[Authorize]
public class WishlistController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public WishlistController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await _mediator.Send(new GetMyWishlistQuery(CurrentUserId), ct));

    [HttpPost("{productId:guid}")]
    public async Task<IActionResult> AddItem(Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddToWishlistCommand(CurrentUserId, productId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveFromWishlistCommand(CurrentUserId, productId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }
}

