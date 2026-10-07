using KidsPocket.Api.Auth;
using KidsPocket.Application.Features.Ledger.GetChildBalance;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/children/{childId:guid}")]
[Authorize]
public class LedgerController : ControllerBase
{
    private readonly GetChildBalanceHandler _handler;
    private readonly KidsPocketDbContext _db;

    public LedgerController(GetChildBalanceHandler handler, KidsPocketDbContext db)
    {
        _handler = handler;
        _db = db;
    }

    [HttpGet("balance")]
    public async Task<ActionResult<ChildBalanceResult>> GetBalance(Guid childId)
    {
        if (!await User.CanAccessChildAsync(childId, _db))
            return Forbid();

        return Ok(await _handler.HandleAsync(new GetChildBalanceQuery(childId)));
    }
}
