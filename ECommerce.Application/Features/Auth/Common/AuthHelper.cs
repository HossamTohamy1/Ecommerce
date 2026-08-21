using ECommerce.Application.DTOs.Auth;
using ECommerce.Domain.Entities;
using ECommerce.Domain.ValueObjects;
using ECommerce.Shared.Common;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Features.Auth.Common;

public static class AuthHelper
{
    public const string DefaultRole = AppConstants.Roles.Customer;

    public static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);
    public static readonly TimeSpan EmailConfirmationLifetime = TimeSpan.FromHours(24);
    public static readonly TimeSpan TwoFactorCodeLifetime = TimeSpan.FromMinutes(5);

    public static async Task<AuthResponse> IssueAuthResponseAsync(
        ApplicationUser user,
        IList<string> roles,
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        CancellationToken ct)
    {
        var token = jwtTokenService.GenerateToken(user, roles);

        var rawRefreshToken = TokenHasher.GenerateRawToken();
        var refreshTokenExpiresAtUtc = DateTime.UtcNow.Add(RefreshTokenLifetime);

        context.Set<RefreshToken>().Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawRefreshToken),
            ExpiresAtUtc = refreshTokenExpiresAtUtc
        });
        await context.SaveChangesAsync(ct);

        var response = user.Adapt<AuthResponse>();
        response.FullName = user.FullName;
        response.Roles = roles;
        response.AccessToken = token.AccessToken;
        response.ExpiresAtUtc = token.ExpiresAtUtc;
        response.RefreshToken = rawRefreshToken;
        response.RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc;
        return response;
    }

    public static async Task<TwoFactorCode> IssueTwoFactorChallengeAsync(
        ApplicationUser user,
        IApplicationDbContext context,
        IEmailService emailService,
        CancellationToken ct)
    {
        var code = Random.Shared.Next(0, 1_000_000).ToString("D6");

        var challenge = new TwoFactorCode
        {
            UserId = user.Id,
            CodeHash = TokenHasher.Hash(code),
            ExpiresAtUtc = DateTime.UtcNow.Add(TwoFactorCodeLifetime)
        };

        context.Set<TwoFactorCode>().Add(challenge);
        await context.SaveChangesAsync(ct);

        await emailService.SendTwoFactorCodeAsync(user.Email!, user.FullName, code, ct);

        return challenge;
    }

    public static async Task SendEmailConfirmationAsync(
        ApplicationUser user,
        IApplicationDbContext context,
        IEmailService emailService,
        CancellationToken ct)
    {
        var activeTokens = await context.Set<EmailConfirmationToken>()
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(ct);
        foreach (var old in activeTokens)
        {
            old.IsUsed = true;
        }

        var rawToken = TokenHasher.GenerateRawToken();
        context.Set<EmailConfirmationToken>().Add(new EmailConfirmationToken
        {
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.Add(EmailConfirmationLifetime)
        });
        await context.SaveChangesAsync(ct);

        await emailService.SendEmailConfirmationAsync(user.Email!, user.FullName, user.Id, rawToken, ct);
    }
}
