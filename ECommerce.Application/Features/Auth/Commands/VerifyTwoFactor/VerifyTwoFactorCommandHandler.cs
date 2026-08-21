using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Common;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.VerifyTwoFactor;

public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public VerifyTwoFactorCommandHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IStringLocalizer<SharedResource> localizer)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenService = jwtTokenService;
        _localizer = localizer;
    }

    public async Task<Result<AuthResponse>> Handle(VerifyTwoFactorCommand command, CancellationToken ct)
    {
        var challenge = await _context.Set<TwoFactorCode>()
            .FirstOrDefaultAsync(c => c.Id == command.Request.ChallengeId, ct);

        if (challenge is null)
        {
            return Result<AuthResponse>.Failure(_localizer["Auth.TwoFactor.ChallengeNotFound"].Value);
        }

        if (!challenge.IsValid)
        {
            return challenge.AttemptCount >= 5
                ? Result<AuthResponse>.Failure(_localizer["Auth.TwoFactor.TooManyAttempts"].Value)
                : Result<AuthResponse>.Failure(_localizer["Auth.TwoFactor.InvalidCode"].Value);
        }

        var codeHash = TokenHasher.Hash(command.Request.Code.Trim());
        if (codeHash != challenge.CodeHash)
        {
            challenge.AttemptCount++;
            await _context.SaveChangesAsync(ct);
            return Result<AuthResponse>.Failure(_localizer["Auth.TwoFactor.InvalidCode"].Value);
        }

        var user = await _userManager.FindByIdAsync(challenge.UserId.ToString());
        if (user is null)
        {
            return Result<AuthResponse>.Failure(_localizer["Auth.TwoFactor.ChallengeNotFound"].Value);
        }

        challenge.IsUsed = true;
        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync(ct);

        var roles = await _userManager.GetRolesAsync(user);
        return Result<AuthResponse>.Success(await AuthHelper.IssueAuthResponseAsync(user, roles, _context, _jwtTokenService, ct));
    }
}
