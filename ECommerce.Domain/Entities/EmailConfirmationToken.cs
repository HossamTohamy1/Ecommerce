namespace ECommerce.Domain.Entities;

/// <summary>
/// One-time token emailed to a user to confirm ownership of their email address.
/// Mirrors PasswordResetToken's shape/lifecycle for consistency across the auth flows.
/// </summary>
public class EmailConfirmationToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsValid => !IsUsed && ExpiresAtUtc > DateTime.UtcNow;
}
