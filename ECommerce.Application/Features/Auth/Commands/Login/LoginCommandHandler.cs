using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Common;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IStringLocalizer<SharedResource> localizer,
        ILogger<LoginCommandHandler> logger)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        var invalidCredentialsMessage = _localizer["Auth.Login.InvalidCredentials"].Value;

        if (user is null)
        {
            return Result<AuthResponse>.Failure(invalidCredentialsMessage);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            var minutesLeft = user.LockoutEnd.HasValue
                ? Math.Ceiling((user.LockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                : 0;
            return Result<AuthResponse>.Failure(_localizer["Auth.Login.LockedOut", minutesLeft].Value);
        }

        if (!await _userManager.CheckPasswordAsync(user, command.Request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            _logger.LogWarning("Failed login attempt for {Email}", normalizedEmail);
            return Result<AuthResponse>.Failure(invalidCredentialsMessage);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        if (user.TwoFactorEnabled)
        {
            var challenge = await AuthHelper.IssueTwoFactorChallengeAsync(user, _context, _emailService, ct);

            var pending = user.Adapt<AuthResponse>();
            pending.FullName = user.FullName;
            pending.RequiresTwoFactor = true;
            pending.TwoFactorChallengeId = challenge.Id;
            return Result<AuthResponse>.Success(pending);
        }

        user.LastLoginAtUtc = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        return Result<AuthResponse>.Success(await AuthHelper.IssueAuthResponseAsync(user, roles, _context, _jwtTokenService, ct));
    }
}
