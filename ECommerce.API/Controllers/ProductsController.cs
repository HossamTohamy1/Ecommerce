
using ECommerce.Application.DTOs.Catalog;
using ECommerce.Application.Features.Products.Commands.AddProductVariant;
using ECommerce.Application.Features.Products.Commands.CreateProduct;
using ECommerce.Application.Features.Products.Commands.DeleteProduct;
using ECommerce.Application.Features.Products.Commands.DeleteProductImage;
using ECommerce.Application.Features.Products.Commands.DeleteProductVariant;
using ECommerce.Application.Features.Products.Commands.SetMainProductImage;
using ECommerce.Application.Features.Products.Commands.UpdateProduct;
using ECommerce.Application.Features.Products.Commands.UpdateProductVariant;
using ECommerce.Application.Features.Products.Commands.UploadProductImages;
using ECommerce.Application.Features.Products.Queries.GetProductById;
using ECommerce.Application.Features.Products.Queries.GetProducts;

namespace ECommerce.API.Controllers;

[Route("api/products")]
public class ProductsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] ProductListQuery query, CancellationToken ct)
        => Ok(await _mediator.Send(new GetProductsQuery(query), ct));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), ct);
        return result.Succeeded ? Ok(result.Data) : NotFound(new { message = result.Error });
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateProductRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateProductCommand(request, CurrentUserId), ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new { message = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProductCommand(id, request, CurrentUserId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteProductCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }

    [HttpPost("{id:guid}/images")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadImages(Guid id, List<IFormFile> images, CancellationToken ct)
    {
        var result = await _mediator.Send(new UploadProductImagesCommand(id, images, CurrentUserId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> DeleteImage(Guid id, Guid imageId, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteProductImageCommand(id, imageId), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }

    [HttpPatch("{id:guid}/images/{imageId:guid}/set-main")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> SetMainImage(Guid id, Guid imageId, CancellationToken ct)
    {
        var result = await _mediator.Send(new SetMainProductImageCommand(id, imageId), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }

    [HttpPost("{id:guid}/variants")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> AddVariant(Guid id, [FromBody] CreateProductVariantRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddProductVariantCommand(id, request, CurrentUserId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpPut("{id:guid}/variants/{variantId:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> UpdateVariant(Guid id, Guid variantId, [FromBody] UpdateProductVariantRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateProductVariantCommand(id, variantId, request, CurrentUserId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}/variants/{variantId:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> DeleteVariant(Guid id, Guid variantId, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteProductVariantCommand(id, variantId), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }
}

