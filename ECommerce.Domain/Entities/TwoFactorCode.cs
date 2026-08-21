namespace ECommerce.Domain.Entities;

/// <summary>
/// A short-lived one-time code emailed to a user during 2FA-protected login.
/// Only the hash of the code is stored; the raw code is emailed once and never persisted.
/// </summary>
public class TwoFactorCode
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; }

    /// <summary>Number of failed verification attempts against this code.</summary>
    public int AttemptCount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    private const int MaxAttempts = 5;

    public bool IsValid => !IsUsed && ExpiresAtUtc > DateTime.UtcNow && AttemptCount < MaxAttempts;
}
