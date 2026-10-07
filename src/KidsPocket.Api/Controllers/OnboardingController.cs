using KidsPocket.Api.Auth;
using KidsPocket.Application.Features.Household.GetHouseholdChildren;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

// Bootstrap ליצירת בית אב / מבוגר / ילד. יצירת בית אב + הוספת המבוגר הראשון הן ה-"הרשמה" -
// לכן נשארות פתוחות (Anonymous); כל השאר דורש טוקן הורה תקף לבית האב הזה.
[ApiController]
[Route("api")]
public class OnboardingController : ControllerBase
{
    private readonly KidsPocketDbContext _db;
    private readonly GetHouseholdChildrenHandler _getChildren;

    public OnboardingController(KidsPocketDbContext db, GetHouseholdChildrenHandler getChildren)
    {
        _db = db;
        _getChildren = getChildren;
    }

    public record CreateHouseholdRequest(string Name);

    [HttpPost("households")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateHousehold(CreateHouseholdRequest request)
    {
        var household = Household.Create(request.Name);
        _db.Households.Add(household);
        await _db.SaveChangesAsync();
        return Ok(new { id = household.Id, household.Name });
    }

    public record AddAdultRequest(string DisplayName, string Role, string Email, string Password);

    [HttpPost("households/{householdId:guid}/adults")]
    [AllowAnonymous]
    public async Task<IActionResult> AddAdult(Guid householdId, AddAdultRequest request)
    {
        if (!Enum.TryParse<AdultRole>(request.Role, ignoreCase: true, out var role))
            return BadRequest(new { error = $"תפקיד לא מוכר: {request.Role}" });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var adult = Adult.Create(householdId, request.DisplayName, role, request.Email, passwordHash);
        _db.Adults.Add(adult);
        await _db.SaveChangesAsync();
        return Ok(new { id = adult.Id, adult.DisplayName, Role = adult.Role.ToString() });
    }

    public record AddChildRequest(string DisplayName, string? Pin);

    [HttpPost("households/{householdId:guid}/children")]
    [Authorize(Roles = AppRoles.Parent)]
    public async Task<IActionResult> AddChild(Guid householdId, AddChildRequest request)
    {
        if (User.GetHouseholdId() != householdId)
            return Forbid();

        var child = Domain.Entities.Child.Create(request.DisplayName);
        if (!string.IsNullOrWhiteSpace(request.Pin))
            child.SetPin(BCrypt.Net.BCrypt.HashPassword(request.Pin));
        _db.Children.Add(child);

        var ledger = Ledger.CreateForChild(child.Id);
        _db.Ledgers.Add(ledger);

        var membership = ChildAdult.Create(child.Id, householdId);
        _db.ChildAdults.Add(membership);

        await _db.SaveChangesAsync();
        return Ok(new { id = child.Id, child.DisplayName, ledgerId = ledger.Id });
    }

    [HttpGet("households/{householdId:guid}/children")]
    [Authorize(Roles = AppRoles.Parent)]
    public async Task<ActionResult<HouseholdChildrenResult>> GetChildren(Guid householdId)
    {
        if (User.GetHouseholdId() != householdId)
            return Forbid();

        return Ok(await _getChildren.HandleAsync(new GetHouseholdChildrenQuery(householdId)));
    }
}
