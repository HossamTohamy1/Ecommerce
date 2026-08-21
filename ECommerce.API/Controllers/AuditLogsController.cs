using ECommerce.Application.DTOs.Audit;
using ECommerce.Application.Features.AuditLogs.Queries.GetPagedAuditLogs;

namespace ECommerce.API.Controllers;

[Route("api/audit-logs")]
[Authorize(Roles = AppConstants.Roles.Admin)]
public class AuditLogsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] AuditLogFilter filter,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
        => Ok(await _mediator.Send(new GetPagedAuditLogsQuery(filter, page, pageSize), ct));
}

