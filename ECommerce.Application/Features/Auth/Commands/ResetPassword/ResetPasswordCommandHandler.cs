using ECommerce.Application.DTOs.Auth;
using ECommerce.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        IStringLocalizer<SharedResource> localizer,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _context = context;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        var invalidTokenMessage = _localizer["Auth.ResetPassword.InvalidToken"].Value;

        if (user is null)
        {
            return Result.Failure(invalidTokenMessage);
        }

        var tokenHash = TokenHasher.Hash(command.Request.Token);
        var validToken = await _context.Set<PasswordResetToken>()
            .Where(t => t.UserId == user.Id
                        && t.TokenHash == tokenHash
                        && !t.IsUsed
                        && t.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (validToken is null)
        {
            return Result.Failure(invalidTokenMessage);
        }

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            return Result.Failure(removeResult.Errors.Select(e => e.Description));
        }

        var addResult = await _userManager.AddPasswordAsync(user, command.Request.NewPassword);
        if (!addResult.Succeeded)
        {
            return Result.Failure(addResult.Errors.Select(e => e.Description));
        }

        await _userManager.UpdateSecurityStampAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);

        validToken.IsUsed = true;

        var activeRefreshTokens = await _context.Set<ECommerce.Domain.Entities.RefreshToken>()
            .Where(t => t.UserId == user.Id && t.RevokedAtUtc == null && t.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(ct);
        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.RevokedAtUtc = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Password reset completed for {Email}", normalizedEmail);
        return Result.Success();
    }
}
