using ECommerce.Application.DTOs.Audit;

namespace ECommerce.Application.Features.AuditLogs.Queries.GetPagedAuditLogs;

public record GetPagedAuditLogsQuery(AuditLogFilter Filter, int Page, int PageSize) : IRequest<PagedResult<AuditLogDto>>;
