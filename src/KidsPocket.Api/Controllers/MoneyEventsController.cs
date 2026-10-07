using KidsPocket.Api.Auth;
using KidsPocket.Application.Features.Allowance.ReceiveAllowance;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/money-events")]
[Authorize(Roles = AppRoles.Parent)]
public class MoneyEventsController : ControllerBase
{
    private readonly ReceiveAllowanceHandler _handler;
    private readonly KidsPocketDbContext _db;

    public MoneyEventsController(ReceiveAllowanceHandler handler, KidsPocketDbContext db)
    {
        _handler = handler;
        _db = db;
    }

    public record CreateMoneyEventRequest(Guid ChildId, string Source, decimal Amount, string? Description);

    // כל כניסת כסף - דמי כיס, מתנה, בונוס - עוברת דרך כאן. פותח Decision אוטומטית.
    // רק הורה יכול "לתת" כסף, ורק לילד שמקושר לבית האב שלו.
    [HttpPost]
    public async Task<ActionResult<ReceiveAllowanceResult>> Create(CreateMoneyEventRequest request)
    {
        if (!await User.CanAccessChildAsync(request.ChildId, _db))
            return Forbid();

        var result = await _handler.HandleAsync(new ReceiveAllowanceCommand(request.ChildId, request.Source, request.Amount, request.Description));
        return Ok(result);
    }
}
