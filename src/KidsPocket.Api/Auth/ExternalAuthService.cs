using System.IdentityModel.Tokens.Jwt;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;

namespace KidsPocket.Api.Auth;

public record ExternalIdentity(string ExternalId, string? Email, string? DisplayName);

public class ExternalAuthException(string message) : Exception(message);

// אימות ID token מ-Google/Apple. שני הספקים שולחים JWT חתום - Google מגיע עם ספריית
// Google.Apis.Auth מוכנה, ל-Apple אין SDK רשמי ל-.NET אז מאמתים ידנית מול ה-JWKS הציבורי שלהם.
public class ExternalAuthService
{
    private const string AppleIssuer = "https://appleid.apple.com";
    private const string AppleJwksUrl = "https://appleid.apple.com/auth/keys";

    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public ExternalAuthService(IConfiguration config, HttpClient http)
    {
        _config = config;
        _http = http;
    }

    public async Task<ExternalIdentity> ValidateGoogleTokenAsync(string idToken)
    {
        var clientId = _config["GoogleAuth:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ExternalAuthException("התחברות Google לא מוגדרת בשרת (חסר GoogleAuth:ClientId)");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            });
        }
        catch (InvalidJwtException ex)
        {
            throw new ExternalAuthException($"טוקן Google לא תקין: {ex.Message}");
        }

        return new ExternalIdentity(payload.Subject, payload.Email, payload.Name);
    }

    public async Task<ExternalIdentity> ValidateAppleTokenAsync(string idToken)
    {
        var clientId = _config["AppleAuth:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ExternalAuthException("התחברות Apple לא מוגדרת בשרת (חסר AppleAuth:ClientId)");

        var signingKeys = await GetAppleSigningKeysAsync();

        var handler = new JwtSecurityTokenHandler();
        System.Security.Claims.ClaimsPrincipal principal;
        try
        {
            principal = handler.ValidateToken(idToken, new TokenValidationParameters
            {
                ValidIssuer = AppleIssuer,
                ValidAudience = clientId,
                IssuerSigningKeys = signingKeys,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromMinutes(1),
            }, out _);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            throw new ExternalAuthException($"טוקן Apple לא תקין: {ex.Message}");
        }

        var subject = principal.FindFirst("sub")?.Value
            ?? throw new ExternalAuthException("טוקן Apple חסר sub");
        var email = principal.FindFirst("email")?.Value;

        return new ExternalIdentity(subject, email, DisplayName: null);
    }

    private async Task<IEnumerable<SecurityKey>> GetAppleSigningKeysAsync()
    {
        // לא cache-ים בכוונה - קריאה יחידה ל-login, מספר ה-logins הצפוי ב-MVP קטן מספיק
        // שלא שווה לסבך עם IMemoryCache פה. אם זה יהפוך ל-hot path אפשר להוסיף cache עם TTL.
        var response = await _http.GetAsync(AppleJwksUrl);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return new JsonWebKeySet(json).GetSigningKeys();
    }
}
