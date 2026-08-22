namespace ECommerce.Domain.Entities;

public class TwoFactorCode
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; }

    public int AttemptCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    private const int MaxAttempts = 5;

    public bool IsValid => !IsUsed && ExpiresAtUtc > DateTime.UtcNow && AttemptCount < MaxAttempts;
}
