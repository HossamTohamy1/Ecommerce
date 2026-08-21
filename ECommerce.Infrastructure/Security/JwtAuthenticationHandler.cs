using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Infrastructure.Security;

public class JwtAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "Bearer";
}

public class JwtAuthenticationHandler : AuthenticationHandler<JwtAuthenticationOptions>
{
    private readonly JwtValidator _validator;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public JwtAuthenticationHandler(
        IOptionsMonitor<JwtAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        JwtValidator validator,
        IStringLocalizer<SharedResource> localizer)
        : base(options, logger, encoder)
    {
        _validator = validator;
        _localizer = localizer;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = ExtractToken();
        if (token is null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.Fail(_localizer["Jwt.NoToken"].Value));
        }

        var result = _validator.Validate(token);
        if (!result.IsValid || result.Claims is null)
        {
            return Task.FromResult(AuthenticateResult.Fail(result.FailureReason ?? _localizer["Jwt.Invalid"].Value));
        }

        var claims = new List<Claim>();
        if (result.Claims.TryGetValue("sub", out var sub))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.GetString() ?? string.Empty));
        if (result.Claims.TryGetValue("email", out var email))
            claims.Add(new Claim(ClaimTypes.Email, email.GetString() ?? string.Empty));
        if (result.Claims.TryGetValue("name", out var name))
            claims.Add(new Claim(ClaimTypes.Name, name.GetString() ?? string.Empty));
        if (result.Claims.TryGetValue("roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var roleElement in rolesElement.EnumerateArray())
            {
                var roleName = roleElement.GetString();
                if (!string.IsNullOrEmpty(roleName))
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }

        var identity = new ClaimsIdentity(claims, JwtAuthenticationOptions.SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, JwtAuthenticationOptions.SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private string? ExtractToken()
    {
        if (Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var headerValue = authHeader.ToString();
            if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return headerValue["Bearer ".Length..].Trim();
            }
        }

        if (Request.Path.StartsWithSegments("/hubs") && Request.Query.TryGetValue("access_token", out var queryToken))
        {
            return queryToken.ToString();
        }

        return null;
    }
}
