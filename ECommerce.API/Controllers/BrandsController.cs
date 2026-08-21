
using ECommerce.Application.Features.Brands.Commands.CreateBrand;
using ECommerce.Application.Features.Brands.Commands.DeleteBrand;
using ECommerce.Application.Features.Brands.Commands.UpdateBrand;
using ECommerce.Application.Features.Brands.Commands.UploadBrandLogo;
using ECommerce.Application.Features.Brands.Queries.GetAllBrands;
using ECommerce.Application.Features.Brands.Queries.GetBrandById;

namespace ECommerce.API.Controllers;

[Route("api/brands")]
public class BrandsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public BrandsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _mediator.Send(new GetAllBrandsQuery(), ct));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBrandByIdQuery(id), ct);
        return result.Succeeded ? Ok(result.Data) : NotFound(new { message = result.Error });
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateBrandRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateBrandCommand(request, CurrentUserId), ct);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
            : BadRequest(new { message = result.Error });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBrandRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateBrandCommand(id, request, CurrentUserId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpPost("{id:guid}/logo")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo(Guid id, IFormFile logo, CancellationToken ct)
    {
        var result = await _mediator.Send(new UploadBrandLogoCommand(id, logo, CurrentUserId), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = AppConstants.Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteBrandCommand(id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }
}

