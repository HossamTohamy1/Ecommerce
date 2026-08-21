using ECommerce.Application.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.EnableTwoFactor;

public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<EnableTwoFactorCommandHandler> _logger;

    public EnableTwoFactorCommandHandler(
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<SharedResource> localizer,
        ILogger<EnableTwoFactorCommandHandler> logger)
    {
        _userManager = userManager;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result> Handle(EnableTwoFactorCommand command, CancellationToken ct)
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

        if (user.TwoFactorEnabled)
        {
            return Result.Failure(_localizer["Auth.TwoFactor.AlreadyEnabled"].Value);
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        _logger.LogInformation("2FA enabled for {UserId}", command.UserId);
        return Result.Success();
    }
}
