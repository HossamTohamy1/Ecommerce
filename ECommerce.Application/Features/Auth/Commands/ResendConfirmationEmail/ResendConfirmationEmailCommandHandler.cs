using ECommerce.Application.Features.Auth.Common;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand, Result>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public ResendConfirmationEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        IEmailService emailService)
    {
        _userManager = userManager;
        _context = context;
        _emailService = emailService;
    }

    public async Task<Result> Handle(ResendConfirmationEmailCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(normalizedEmail);

        if (user is not null && !user.EmailConfirmed)
        {
            await AuthHelper.SendEmailConfirmationAsync(user, _context, _emailService, ct);
        }

        return Result.Success();
    }
}
