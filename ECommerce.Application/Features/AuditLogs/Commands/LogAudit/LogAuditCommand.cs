namespace ECommerce.Application.Features.AuditLogs.Commands.LogAudit;

public record LogAuditCommand(
    string? UserId,
    string? UserName,
    AuditAction Action,
    string EntityName,
    string EntityId,
    string? Description,
    string? IpAddress) : IRequest;
