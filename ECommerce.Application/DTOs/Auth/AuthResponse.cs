namespace ECommerce.Application.DTOs.Auth;

public class AuthResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();

    // Empty/default when RequiresTwoFactor is true — the client must call /api/auth/2fa/verify first.
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAtUtc { get; set; }

    /// <summary>True when the account has 2FA enabled and login must be completed via /api/auth/2fa/verify.</summary>
    public bool RequiresTwoFactor { get; set; }

    /// <summary>Opaque id identifying the pending 2FA challenge, only set when RequiresTwoFactor is true.</summary>
    public Guid? TwoFactorChallengeId { get; set; }
}

public class UserProfileResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAtUtc { get; set; }
}
