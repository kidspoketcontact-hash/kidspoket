using KidsPocket.Application.Features.Allowance.ReceiveAllowance;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/money-events")]
public class MoneyEventsController : ControllerBase
{
    private readonly ReceiveAllowanceHandler _handler;
    public MoneyEventsController(ReceiveAllowanceHandler handler) => _handler = handler;

    public record CreateMoneyEventRequest(Guid ChildId, string Source, decimal Amount, string? Description);

    // כל כניסת כסף - דמי כיס, מתנה, בונוס - עוברת דרך כאן. פותח Decision אוטומטית.
    [HttpPost]
    public async Task<ActionResult<ReceiveAllowanceResult>> Create(CreateMoneyEventRequest request)
    {
        var result = await _handler.HandleAsync(new ReceiveAllowanceCommand(request.ChildId, request.Source, request.Amount, request.Description));
        return Ok(result);
    }
}
