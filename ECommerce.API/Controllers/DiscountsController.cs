using ECommerce.Application.DTOs.Discounts;
using ECommerce.Application.Features.Discounts.Commands.AssignProductToDiscount;
using ECommerce.Application.Features.Discounts.Commands.CreateDiscount;
using ECommerce.Application.Features.Discounts.Commands.DeleteDiscount;
using ECommerce.Application.Features.Discounts.Commands.RemoveProductFromDiscount;
using ECommerce.Application.Features.Discounts.Commands.UpdateDiscount;
using ECommerce.Application.Features.Discounts.Queries.GetAllDiscounts;
using ECommerce.Application.Features.Discounts.Queries.GetDiscountById;

namespace ECommerce.API.Controllers;

[Route("api/discounts")]
[Authorize(Roles = AppConstants.Roles.Admin)]
public class DiscountsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public DiscountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllDiscountsQuery(), ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDiscountByIdQuery(id), ct);
        return result.Succeeded ? Ok(result.Data) : NotFound(new { message = result.Error });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDiscountRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateDiscountCommand(request, CurrentUserId), ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new { message = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDiscountRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateDiscountCommand(id, request, CurrentUserId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteDiscountCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }

    [HttpPost("{id:guid}/products")]
    public async Task<IActionResult> AssignProduct(Guid id, [FromBody] AssignProductToDiscountRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AssignProductToDiscountCommand(id, request.ProductId, CurrentUserId), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}/products/{productId:guid}")]
    public async Task<IActionResult> RemoveProduct(Guid id, Guid productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveProductFromDiscountCommand(id, productId), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }
}

