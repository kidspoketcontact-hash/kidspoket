using KidsPocket.Api.Auth;
using KidsPocket.Application.Features.Reward.CreateReward;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/rewards")]
[Authorize(Roles = AppRoles.Parent)]
public class RewardsController : ControllerBase
{
    private readonly CreateRewardHandler _handler;
    private readonly KidsPocketDbContext _db;

    public RewardsController(CreateRewardHandler handler, KidsPocketDbContext db)
    {
        _handler = handler;
        _db = db;
    }

    [HttpPost]
    public async Task<ActionResult<CreateRewardResult>> Create(CreateRewardCommand command)
    {
        if (!await User.CanAccessChildAsync(command.ChildId, _db))
            return Forbid();

        return Ok(await _handler.HandleAsync(command));
    }
}
