using ECommerce.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.DisableTwoFactor;

public class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<DisableTwoFactorCommandHandler> _logger;

    public DisableTwoFactorCommandHandler(
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<SharedResource> localizer,
        ILogger<DisableTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result> Handle(DisableTwoFactorCommand command, CancellationToken ct)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user is null)
        {
            return Result.Failure(_localizer["Auth.User.NotFound"].Value);
        }

        if (!await _userManager.CheckPasswordAsync(user, command.Request.CurrentPassword))
        {
            return Result.Failure(_localizer["Auth.TwoFactor.WrongPassword"].Value);
        }

        if (!user.TwoFactorEnabled)
        {
            return Result.Failure(_localizer["Auth.TwoFactor.AlreadyDisabled"].Value);
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        _logger.LogInformation("2FA disabled for {UserId}", command.UserId);
        return Result.Success();
    }
}
