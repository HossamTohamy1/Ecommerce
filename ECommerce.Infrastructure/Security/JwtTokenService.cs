using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECommerce.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public GeneratedToken GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(_settings.ExpiryMinutes);

        var header = new Dictionary<string, object?>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object?>
        {
            ["sub"] = user.Id.ToString(),
            ["email"] = user.Email,
            ["name"] = user.FullName,
            ["roles"] = roles,
            ["sstamp"] = user.SecurityStamp,
            ["jti"] = Guid.NewGuid().ToString("N"),
            ["iss"] = _settings.Issuer,
            ["aud"] = _settings.Audience,
            ["iat"] = ToUnixSeconds(now),
            ["nbf"] = ToUnixSeconds(now),
            ["exp"] = ToUnixSeconds(expires)
        };

        var headerSegment = Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(header));
        var payloadSegment = Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(payload));
        var unsignedToken = $"{headerSegment}.{payloadSegment}";

        var signature = Sign(unsignedToken);
        var token = $"{unsignedToken}.{signature}";

        return new GeneratedToken(token, expires);
    }

    private string Sign(string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Base64Url.EncodeToString(hash);
    }

    private static long ToUnixSeconds(DateTime dt) => new DateTimeOffset(dt).ToUnixTimeSeconds();
}
