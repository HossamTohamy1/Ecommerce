using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Common;
using Mapster;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.ExternalLogin;

public class ExternalLoginCommandHandler : IRequestHandler<ExternalLoginCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<ExternalLoginCommandHandler> _logger;

    public ExternalLoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IStringLocalizer<SharedResource> localizer,
        ILogger<ExternalLoginCommandHandler> logger)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(ExternalLoginCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Request.Email.Trim().ToLowerInvariant();
        var loginInfo = new UserLoginInfo(command.Request.Provider, command.Request.ProviderKey, command.Request.Provider);

        var user = await _userManager.FindByLoginAsync(command.Request.Provider, command.Request.ProviderKey);

        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(normalizedEmail);

            if (user is not null)
            {
                var linkResult = await _userManager.AddLoginAsync(user, loginInfo);
                if (!linkResult.Succeeded)
                {
                    return Result<AuthResponse>.Failure(linkResult.Errors.Select(e => e.Description));
                }
            }
            else
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = normalizedEmail,
                    Email = normalizedEmail,
                    EmailConfirmed = true,
                    FullName = string.IsNullOrWhiteSpace(command.Request.FullName) ? normalizedEmail : command.Request.FullName.Trim(),
                    CreatedAtUtc = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return Result<AuthResponse>.Failure(createResult.Errors.Select(e => e.Description));
                }

                await _userManager.AddToRoleAsync(user, AuthHelper.DefaultRole);

                var linkResult = await _userManager.AddLoginAsync(user, loginInfo);
                if (!linkResult.Succeeded)
                {
                    return Result<AuthResponse>.Failure(linkResult.Errors.Select(e => e.Description));
                }

                _logger.LogInformation("New user registered via {Provider}: {Email}", command.Request.Provider, normalizedEmail);
            }
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            var minutesLeft = user.LockoutEnd.HasValue
                ? Math.Ceiling((user.LockoutEnd.Value - DateTimeOffset.UtcNow).TotalMinutes)
                : 0;
            return Result<AuthResponse>.Failure(_localizer["Auth.Login.LockedOut", minutesLeft].Value);
        }

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
