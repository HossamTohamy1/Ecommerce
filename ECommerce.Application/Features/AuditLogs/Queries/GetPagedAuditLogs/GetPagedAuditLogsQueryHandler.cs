using ECommerce.Application.DTOs.Audit;

namespace ECommerce.Application.Features.AuditLogs.Queries.GetPagedAuditLogs;

public class GetPagedAuditLogsQueryHandler : IRequestHandler<GetPagedAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPagedAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditLogDto>> Handle(GetPagedAuditLogsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 50 : request.PageSize;

        var query = _context.Set<AuditLog>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Filter.EntityName))
        {
            query = query.Where(a => a.EntityName == request.Filter.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(request.Filter.UserId))
        {
            query = query.Where(a => a.UserId == request.Filter.UserId);
        }

        if (request.Filter.Action is not null)
        {
            query = query.Where(a => a.Action == request.Filter.Action);
        }

        if (request.Filter.FromUtc is not null)
        {
            query = query.Where(a => a.Timestamp >= request.Filter.FromUtc);
        }

        if (request.Filter.ToUtc is not null)
        {
            query = query.Where(a => a.Timestamp <= request.Filter.ToUtc);
        }

        query = query.OrderByDescending(a => a.Timestamp);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                UserId = a.UserId,
                UserName = a.UserName,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Changes = a.Changes,
                Description = a.Description,
                IpAddress = a.IpAddress
            })
            .ToListAsync(ct);

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
