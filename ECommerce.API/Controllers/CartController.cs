
using ECommerce.Application.Features.Carts.Commands.AddToCart;
using ECommerce.Application.Features.Carts.Commands.ClearCart;
using ECommerce.Application.Features.Carts.Commands.RemoveFromCart;
using ECommerce.Application.Features.Carts.Commands.UpdateCartItem;
using ECommerce.Application.Features.Carts.Queries.GetMyCart;

namespace ECommerce.API.Controllers;

[Route("api/cart")]
[Authorize]
public class CartController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await _mediator.Send(new GetMyCartQuery(CurrentUserId), ct));

    [HttpPost("items")]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddToCartCommand(CurrentUserId, request), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpPut("items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItem(Guid itemId, [FromBody] UpdateCartItemRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateCartItemCommand(CurrentUserId, itemId, request), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveFromCartCommand(CurrentUserId, itemId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken ct)
    {
        await _mediator.Send(new ClearCartCommand(CurrentUserId), ct);
        return NoContent();
    }
}

