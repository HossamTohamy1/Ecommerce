namespace ECommerce.Application.DTOs.Audit;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string? Changes { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
}

public class AuditLogFilter
{
    public string? EntityName { get; set; }
    public string? UserId { get; set; }
    public AuditAction? Action { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
}
