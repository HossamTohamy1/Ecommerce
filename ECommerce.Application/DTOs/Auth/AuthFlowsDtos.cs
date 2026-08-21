namespace ECommerce.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class ConfirmEmailRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;
}

public class ConfirmEmailResponse
{
    public bool AlreadyConfirmed { get; set; }
}

public class VerifyTwoFactorRequest
{
    [Required]
    public Guid ChallengeId { get; set; }

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}

public class EnableTwoFactorRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;
}

/// <summary>
/// Claims handed over by ASP.NET Core's OAuth handlers (Google/Facebook) once the provider
/// redirects back. ProviderKey is the stable per-provider user id (e.g. Google's "sub" claim),
/// never the email, since emails can change or be reused.
/// </summary>
public class ExternalLoginRequest
{
    [Required]
    public string Provider { get; set; } = string.Empty;

    [Required]
    public string ProviderKey { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? FullName { get; set; }
}
