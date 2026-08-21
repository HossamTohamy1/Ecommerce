using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Common;
using ECommerce.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        if (user is null)
        {
            _logger.LogInformation("Password reset requested for unknown email: {Email}", normalizedEmail);
            return Result.Success();
        }

        var activeTokens = await _context.Set<PasswordResetToken>()
            .Where(t => t.UserId == user.Id && !t.IsUsed && t.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync(ct);
        foreach (var old in activeTokens)
        {
            old.IsUsed = true;
        }

        var rawToken = TokenHasher.GenerateRawToken();
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = TokenHasher.Hash(rawToken),
            ExpiresAtUtc = DateTime.UtcNow.Add(AuthHelper.ResetTokenLifetime)
        };

        _context.Set<PasswordResetToken>().Add(resetToken);
        await _context.SaveChangesAsync(ct);

        await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FullName, rawToken, ct);

        return Result.Success();
    }
}
