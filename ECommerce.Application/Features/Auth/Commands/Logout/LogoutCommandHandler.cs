using ECommerce.Application.DTOs.Auth;
using ECommerce.Domain.ValueObjects;

namespace ECommerce.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public LogoutCommandHandler(IApplicationDbContext context, IStringLocalizer<SharedResource> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public async Task<Result> Handle(LogoutCommand command, CancellationToken ct)
    {
        var tokenHash = TokenHasher.Hash(command.Request.RefreshToken);
        var existingToken = await _context.Set<ECommerce.Domain.Entities.RefreshToken>()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (existingToken is null || existingToken.IsRevoked)
        {
            return Result.Failure(_localizer["Auth.Logout.InvalidToken"].Value);
        }

        existingToken.RevokedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
