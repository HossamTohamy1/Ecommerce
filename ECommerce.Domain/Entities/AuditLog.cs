namespace ECommerce.Domain.Entities;

public enum AuditAction
{
    Create = 1,
    Update = 2,
    Delete = 3
}

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }
    public string? UserName { get; set; }

    public AuditAction Action { get; set; }

    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;

    public string? Changes { get; set; }

    public string? Description { get; set; }

    public string? IpAddress { get; set; }
}
