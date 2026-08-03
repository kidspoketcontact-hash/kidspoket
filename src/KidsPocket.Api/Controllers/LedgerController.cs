using KidsPocket.Application.Features.Ledger.GetChildBalance;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/children/{childId:guid}")]
public class LedgerController : ControllerBase
{
    private readonly GetChildBalanceHandler _handler;
    public LedgerController(GetChildBalanceHandler handler) => _handler = handler;

    [HttpGet("balance")]
    public async Task<ActionResult<ChildBalanceResult>> GetBalance(Guid childId) =>
        Ok(await _handler.HandleAsync(new GetChildBalanceQuery(childId)));
}
