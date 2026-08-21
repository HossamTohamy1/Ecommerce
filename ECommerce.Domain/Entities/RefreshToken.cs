namespace ECommerce.Domain.Entities;

/// <summary>
/// A rotating refresh token used to obtain new access tokens without re-entering credentials.
/// Only the hash of the token is persisted (see TokenHasher), the raw value is returned to the
/// client once and never stored, matching the pattern already used by PasswordResetToken.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    /// <summary>Hash of the token that replaced this one, set when rotated.</summary>
    public string? ReplacedByTokenHash { get; set; }

    public bool IsExpired => ExpiresAtUtc <= DateTime.UtcNow;

    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsActive => !IsRevoked && !IsExpired;
}
