using ECommerce.Application.DTOs.Reviews;
using ECommerce.Application.Features.ProductReviews.Commands.ApproveProductReview;
using ECommerce.Application.Features.ProductReviews.Commands.CreateProductReview;
using ECommerce.Application.Features.ProductReviews.Commands.DeleteOwnProductReview;
using ECommerce.Application.Features.ProductReviews.Commands.DeleteProductReviewAsAdmin;
using ECommerce.Application.Features.ProductReviews.Commands.UpdateProductReview;
using ECommerce.Application.Features.ProductReviews.Queries.GetAllProductReviews;
using ECommerce.Application.Features.ProductReviews.Queries.GetApprovedReviewsForProduct;

namespace ECommerce.API.Controllers;

[Route("api/reviews")]
public class ProductReviewsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProductReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetForProduct(Guid productId, CancellationToken ct)
        => Ok(await _mediator.Send(new GetApprovedReviewsForProductQuery(productId), ct));

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateProductReviewRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateProductReviewCommand(CurrentUserId, request), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateOwn(Guid id, [FromBody] UpdateProductReviewRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProductReviewCommand(CurrentUserId, id, request), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteOwn(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteOwnProductReviewCommand(CurrentUserId, id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }

    [HttpGet("admin/all")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> GetAllForModeration(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllProductReviewsQuery(), ct));

    [HttpPut("admin/{id:guid}/approve")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveProductReviewCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }

    [HttpDelete("admin/{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> DeleteAsAdmin(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteProductReviewAsAdminCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }
}

