using KidsPocket.Application.Features.Reward.CreateReward;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/rewards")]
public class RewardsController : ControllerBase
{
    private readonly CreateRewardHandler _handler;
    public RewardsController(CreateRewardHandler handler) => _handler = handler;

    [HttpPost]
    public async Task<ActionResult<CreateRewardResult>> Create(CreateRewardCommand command) =>
        Ok(await _handler.HandleAsync(command));
}
