
namespace ECommerce.Application.Interfaces;

public record GeneratedToken(string AccessToken, DateTime ExpiresAtUtc);

public interface IJwtTokenService
{
    GeneratedToken GenerateToken(ApplicationUser user, IList<string> roles);
}
