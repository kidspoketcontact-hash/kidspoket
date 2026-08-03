using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

// Bootstrap מינימלי ליצירת בית אב / מבוגר / ילד. לא Vertical Slice מלא בכוונה - זה שלד
// לצורך פיתוח והדגמה; לפני production זה צריך לעבור להירשם עם Auth (הרשמה, אימות אימייל וכו').
[ApiController]
[Route("api")]
public class OnboardingController : ControllerBase
{
    private readonly KidsPocketDbContext _db;
    public OnboardingController(KidsPocketDbContext db) => _db = db;

    public record CreateHouseholdRequest(string Name);

    [HttpPost("households")]
    public async Task<IActionResult> CreateHousehold(CreateHouseholdRequest request)
    {
        var household = Household.Create(request.Name);
        _db.Households.Add(household);
        await _db.SaveChangesAsync();
        return Ok(new { id = household.Id, household.Name });
    }

    public record AddAdultRequest(string DisplayName, string Role, string Email, string Password);

    [HttpPost("households/{householdId:guid}/adults")]
    public async Task<IActionResult> AddAdult(Guid householdId, AddAdultRequest request)
    {
        if (!Enum.TryParse<AdultRole>(request.Role, ignoreCase: true, out var role))
            return BadRequest(new { error = $"תפקיד לא מוכר: {request.Role}" });

        // TODO: hash אמיתי (למשל BCrypt) לפני production - כרגע placeholder בלבד
        var passwordHash = $"PLAINTEXT::{request.Password}";
        var adult = Adult.Create(householdId, request.DisplayName, role, request.Email, passwordHash);
        _db.Adults.Add(adult);
        await _db.SaveChangesAsync();
        return Ok(new { id = adult.Id, adult.DisplayName, Role = adult.Role.ToString() });
    }

    public record AddChildRequest(string DisplayName);

    [HttpPost("households/{householdId:guid}/children")]
    public async Task<IActionResult> AddChild(Guid householdId, AddChildRequest request)
    {
        var child = Domain.Entities.Child.Create(request.DisplayName);
        _db.Children.Add(child);

        var ledger = Ledger.CreateForChild(child.Id);
        _db.Ledgers.Add(ledger);

        var membership = ChildAdult.Create(child.Id, householdId);
        _db.ChildAdults.Add(membership);

        await _db.SaveChangesAsync();
        return Ok(new { id = child.Id, child.DisplayName, ledgerId = ledger.Id });
    }
}
