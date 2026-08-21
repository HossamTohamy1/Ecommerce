using ECommerce.Application.DTOs.Auth;
using ECommerce.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<ConfirmEmailResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        IStringLocalizer<SharedResource> localizer,
        ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userManager = userManager;
        _context = context;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result<ConfirmEmailResponse>> Handle(ConfirmEmailCommand command, CancellationToken ct)
    {
        var invalidMessage = _localizer["Auth.ConfirmEmail.Invalid"].Value;

        var user = await _userManager.FindByIdAsync(command.Request.UserId.ToString());
        if (user is null)
        {
            return Result<ConfirmEmailResponse>.Failure(invalidMessage);
        }

        if (user.EmailConfirmed)
        {
            return Result<ConfirmEmailResponse>.Success(new ConfirmEmailResponse { AlreadyConfirmed = true });
        }

        var tokenHash = TokenHasher.Hash(command.Request.Token);
        var validToken = await _context.Set<EmailConfirmationToken>()
            .Where(t => t.UserId == user.Id
                        && t.TokenHash == tokenHash
                        && !t.IsUsed
                        && t.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(t => t.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (validToken is null)
        {
            return Result<ConfirmEmailResponse>.Failure(invalidMessage);
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        validToken.IsUsed = true;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Email confirmed for {UserId}", user.Id);
        return Result<ConfirmEmailResponse>.Success(new ConfirmEmailResponse { AlreadyConfirmed = false });
    }
}
