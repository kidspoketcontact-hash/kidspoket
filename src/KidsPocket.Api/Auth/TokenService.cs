using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace KidsPocket.Api.Auth;

// שני "טיפוסי" משתמש מקבלים טוקנים עם claims שונים - Parent מקבל household_id (הרשאות
// ברמת בית אב), Child מקבל רק את הזהות שלו (הרשאות מוגבלות לילד עצמו).
public class TokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config) => _config = config;

    public string CreateParentToken(Guid adultId, Guid householdId, string displayName) => CreateToken(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, adultId.ToString()),
        new Claim(AppClaimTypes.HouseholdId, householdId.ToString()),
        new Claim(ClaimTypes.Role, AppRoles.Parent),
        new Claim(AppClaimTypes.DisplayName, displayName),
    });

    public string CreateChildToken(Guid childId, string displayName) => CreateToken(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, childId.ToString()),
        new Claim(ClaimTypes.Role, AppRoles.Child),
        new Claim(AppClaimTypes.DisplayName, displayName),
    });

    private string CreateToken(IEnumerable<Claim> claims)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:SigningKey"]!));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiryMinutes = double.Parse(_config["Jwt:ExpiryMinutes"]!);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class AppRoles
{
    public const string Parent = "Parent";
    public const string Child = "Child";
}

public static class AppClaimTypes
{
    public const string HouseholdId = "household_id";
    public const string DisplayName = "display_name";
}
