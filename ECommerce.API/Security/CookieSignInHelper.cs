using System.Security.Claims;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;

namespace ECommerce.API.Security;

public static class CookieSignInHelper
{
    public static async Task SignInAsync(HttpContext httpContext, AuthResponse data)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, data.UserId.ToString()),
            new(ClaimTypes.Email, data.Email),
            new(ClaimTypes.Name, data.FullName)
        };
        claims.AddRange(data.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, CookieAuthDefaults.Scheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(CookieAuthDefaults.Scheme, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        });
    }
}
