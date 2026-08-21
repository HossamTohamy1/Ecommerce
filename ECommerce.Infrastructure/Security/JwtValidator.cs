using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ECommerce.Infrastructure.Security;

public record JwtValidationResult(bool IsValid, string? FailureReason, IReadOnlyDictionary<string, JsonElement>? Claims);

public class JwtValidator
{
    private readonly JwtSettings _settings;
    private readonly IStringLocalizer<SharedResource> _localizer;

    public JwtValidator(IOptions<JwtSettings> settings, IStringLocalizer<SharedResource> localizer)
    {
        _settings = settings.Value;
        _localizer = localizer;
    }

    public JwtValidationResult Validate(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return new JwtValidationResult(false, _localizer["Jwt.MalformedToken"].Value, null);
        }

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = Sign(unsignedToken);

        var providedSigBytes = Base64Url.DecodeFromChars(parts[2]);
        var expectedSigBytes = Base64Url.DecodeFromChars(expectedSignature);
        if (!CryptographicOperations.FixedTimeEquals(providedSigBytes, expectedSigBytes))
        {
            return new JwtValidationResult(false, _localizer["Jwt.InvalidSignature"].Value, null);
        }

        Dictionary<string, JsonElement>? claims;
        try
        {
            var payloadJson = Base64Url.DecodeFromChars(parts[1]);
            claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);
        }
        catch (JsonException)
        {
            return new JwtValidationResult(false, _localizer["Jwt.UnreadablePayload"].Value, null);
        }

        if (claims is null)
        {
            return new JwtValidationResult(false, _localizer["Jwt.UnreadablePayload"].Value, null);
        }

        if (claims.TryGetValue("exp", out var expElement) && expElement.TryGetInt64(out var exp))
        {
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp);
            if (expiresAt < DateTimeOffset.UtcNow)
            {
                return new JwtValidationResult(false, _localizer["Jwt.Expired"].Value, null);
            }
        }
        else
        {
            return new JwtValidationResult(false, _localizer["Jwt.Invalid"].Value, null);
        }

        if (claims.TryGetValue("iss", out var issElement) && issElement.GetString() != _settings.Issuer)
        {
            return new JwtValidationResult(false, _localizer["Jwt.InvalidIssuer"].Value, null);
        }

        if (claims.TryGetValue("aud", out var audElement) && audElement.GetString() != _settings.Audience)
        {
            return new JwtValidationResult(false, _localizer["Jwt.InvalidAudience"].Value, null);
        }

        return new JwtValidationResult(true, null, claims);
    }

    private string Sign(string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Base64Url.EncodeToString(hash);
    }
}
