
using ECommerce.Application.Features.Addresses.Commands.CreateAddress;
using ECommerce.Application.Features.Addresses.Commands.DeleteAddress;
using ECommerce.Application.Features.Addresses.Commands.UpdateAddress;
using ECommerce.Application.Features.Addresses.Queries.GetMyAddresses;

namespace ECommerce.API.Controllers;

[Route("api/addresses")]
[Authorize]
public class AddressesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AddressesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct)
        => Ok(await _mediator.Send(new GetMyAddressesQuery(CurrentUserId), ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveAddressRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateAddressCommand(CurrentUserId, request), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveAddressRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateAddressCommand(CurrentUserId, id, request), ct);
        return result.Succeeded ? Ok(result.Data) : BadRequest(new { message = result.Error });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteAddressCommand(CurrentUserId, id), ct);
        return result.Succeeded ? NoContent() : BadRequest(new { message = result.Error });
    }
}

