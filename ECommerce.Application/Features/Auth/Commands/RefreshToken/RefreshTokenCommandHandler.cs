using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Features.Auth.Common;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public RefreshTokenCommandHandler(
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

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken ct)
    {
        var invalidMessage = _localizer["Auth.RefreshToken.Invalid"].Value;

        var tokenHash = TokenHasher.Hash(command.Request.RefreshToken);
        var existingToken = await _context.Set<ECommerce.Domain.Entities.RefreshToken>()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result<AuthResponse>.Failure(invalidMessage);
        }

        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null)
        {
            return Result<AuthResponse>.Failure(invalidMessage);
        }

        existingToken.RevokedAtUtc = DateTime.UtcNow;

        var roles = await _userManager.GetRolesAsync(user);
        var response = await AuthHelper.IssueAuthResponseAsync(user, roles, _context, _jwtTokenService, ct);

        existingToken.ReplacedByTokenHash = TokenHasher.Hash(response.RefreshToken);
        await _context.SaveChangesAsync(ct);

        return Result<AuthResponse>.Success(response);
    }
}
