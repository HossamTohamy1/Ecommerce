using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Common;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<SharedResource> _localizer;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService,
        IEmailService emailService,
        IStringLocalizer<SharedResource> localizer,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager;
        _context = context;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Request.Email.Trim().ToLowerInvariant();

        var existing = await _userManager.FindByEmailAsync(normalizedEmail);
        if (existing is not null)
        {
            return Result<AuthResponse>.Failure(_localizer["Auth.Register.EmailExists"]);
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = command.Request.FullName.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, command.Request.Password);
        if (!createResult.Succeeded)
        {
            return Result<AuthResponse>.Failure(createResult.Errors.Select(e => e.Description));
        }

        await _userManager.AddToRoleAsync(user, AuthHelper.DefaultRole);

        _logger.LogInformation("New user registered: {Email}", normalizedEmail);

        try
        {
            await AuthHelper.SendEmailConfirmationAsync(user, _context, _emailService, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send confirmation email to {Email}", normalizedEmail);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Result<AuthResponse>.Success(await AuthHelper.IssueAuthResponseAsync(user, roles, _context, _jwtTokenService, ct));
    }
}
