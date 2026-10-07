using KidsPocket.Api.Auth;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly KidsPocketDbContext _db;
    private readonly TokenService _tokens;
    private readonly ExternalAuthService _externalAuth;

    public AuthController(KidsPocketDbContext db, TokenService tokens, ExternalAuthService externalAuth)
    {
        _db = db;
        _tokens = tokens;
        _externalAuth = externalAuth;
    }

    public record ParentLoginRequest(string Email, string Password);
    public record ParentLoginResult(string Token, Guid AdultId, Guid HouseholdId, string DisplayName);

    [HttpPost("parent/login")]
    public async Task<ActionResult<ParentLoginResult>> ParentLogin(ParentLoginRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var adult = await _db.Adults.SingleOrDefaultAsync(a => a.Email.ToLower() == normalizedEmail);

        if (adult is null || adult.PasswordHash is null || !BCrypt.Net.BCrypt.Verify(request.Password, adult.PasswordHash))
            return Unauthorized(new { error = "אימייל או סיסמה שגויים" });

        var token = _tokens.CreateParentToken(adult.Id, adult.HouseholdId, adult.DisplayName);
        return Ok(new ParentLoginResult(token, adult.Id, adult.HouseholdId, adult.DisplayName));
    }

    public record ChildLoginRequest(Guid ChildId, string Pin);
    public record ChildLoginResult(string Token, Guid ChildId, string DisplayName);

    [HttpPost("child/login")]
    public async Task<ActionResult<ChildLoginResult>> ChildLogin(ChildLoginRequest request)
    {
        var child = await _db.Children.SingleOrDefaultAsync(c => c.Id == request.ChildId);

        if (child is null || child.PinHash is null || !BCrypt.Net.BCrypt.Verify(request.Pin, child.PinHash))
            return Unauthorized(new { error = "קוד לא נכון" });

        var token = _tokens.CreateChildToken(child.Id, child.DisplayName);
        return Ok(new ChildLoginResult(token, child.Id, child.DisplayName));
    }

    public record GoogleLoginRequest(string IdToken, string? HouseholdName);

    [HttpPost("google")]
    public async Task<ActionResult<ExternalLoginResult>> GoogleLogin(GoogleLoginRequest request)
    {
        ExternalIdentity identity;
        try
        {
            identity = await _externalAuth.ValidateGoogleTokenAsync(request.IdToken);
        }
        catch (ExternalAuthException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }

        if (identity.Email is null)
            return Unauthorized(new { error = "חשבון Google הזה לא חושף כתובת אימייל" });

        var result = await LoginOrRegisterExternalAsync(
            AuthProvider.Google, identity.ExternalId, identity.Email, identity.DisplayName, request.HouseholdName);
        return Ok(result);
    }

    // Apple שולח email/name רק בפעם הראשונה שהמשתמש מאשר את האפליקציה (בתוך אובייקט "user" נפרד,
    // לא בתוך ה-ID token עצמו) - הלקוח (Web) שולח אותם פה אם הוא קיבל אותם. בפעמים הבאות
    // Email/DisplayName הם null ואנחנו מזהים את המשתמש רק לפי ה-sub שכבר נשמר אצלנו.
    public record AppleLoginRequest(string IdToken, string? HouseholdName, string? Email, string? DisplayName);

    [HttpPost("apple")]
    public async Task<ActionResult<ExternalLoginResult>> AppleLogin(AppleLoginRequest request)
    {
        ExternalIdentity identity;
        try
        {
            identity = await _externalAuth.ValidateAppleTokenAsync(request.IdToken);
        }
        catch (ExternalAuthException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }

        var email = identity.Email ?? request.Email;
        var displayName = identity.DisplayName ?? request.DisplayName;

        var result = await LoginOrRegisterExternalAsync(
            AuthProvider.Apple, identity.ExternalId, email, displayName, request.HouseholdName);
        return Ok(result);
    }

    public record ExternalLoginResult(string Token, Guid AdultId, Guid HouseholdId, string DisplayName, bool IsNewHousehold);

    private async Task<ExternalLoginResult> LoginOrRegisterExternalAsync(
        AuthProvider provider, string externalId, string? email, string? displayName, string? householdName)
    {
        var existingByProvider = await _db.Adults
            .SingleOrDefaultAsync(a => a.Provider == provider && a.ExternalId == externalId);
        if (existingByProvider is not null)
        {
            var providerToken = _tokens.CreateParentToken(existingByProvider.Id, existingByProvider.HouseholdId, existingByProvider.DisplayName);
            return new ExternalLoginResult(providerToken, existingByProvider.Id, existingByProvider.HouseholdId, existingByProvider.DisplayName, IsNewHousehold: false);
        }

        if (email is null)
            throw new ExternalAuthException("חסרה כתובת אימייל לזיהוי המשתמש");

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existingByEmail = await _db.Adults.SingleOrDefaultAsync(a => a.Email.ToLower() == normalizedEmail);
        if (existingByEmail is not null)
        {
            existingByEmail.LinkExternalProvider(provider, externalId);
            await _db.SaveChangesAsync();

            var linkedToken = _tokens.CreateParentToken(existingByEmail.Id, existingByEmail.HouseholdId, existingByEmail.DisplayName);
            return new ExternalLoginResult(linkedToken, existingByEmail.Id, existingByEmail.HouseholdId, existingByEmail.DisplayName, IsNewHousehold: false);
        }

        // אימייל חדש לגמרי - "הרשמה מקומית" אוטומטית: יוצרים בית אב + Adult אמיתיים ב-DB שלנו,
        // בדיוק כמו הרשמה עם אימייל/סיסמה, רק בלי סיסמה כי הזהות מגיעה מ-Google/Apple.
        var name = string.IsNullOrWhiteSpace(displayName) ? email : displayName;
        var household = Household.Create(string.IsNullOrWhiteSpace(householdName) ? $"בית {name}" : householdName);
        _db.Households.Add(household);

        var adult = Adult.CreateExternal(household.Id, name, AdultRole.Guardian, email, provider, externalId);
        _db.Adults.Add(adult);

        await _db.SaveChangesAsync();

        var token = _tokens.CreateParentToken(adult.Id, adult.HouseholdId, adult.DisplayName);
        return new ExternalLoginResult(token, adult.Id, adult.HouseholdId, adult.DisplayName, IsNewHousehold: true);
    }
}
