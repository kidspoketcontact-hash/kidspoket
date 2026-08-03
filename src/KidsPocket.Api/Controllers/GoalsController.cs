using KidsPocket.Application.Features.Goal.ContributeToGoal;
using KidsPocket.Application.Features.Goal.CreateGoal;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/goals")]
public class GoalsController : ControllerBase
{
    private readonly CreateGoalHandler _create;
    private readonly ContributeToGoalHandler _contribute;

    public GoalsController(CreateGoalHandler create, ContributeToGoalHandler contribute)
    {
        _create = create;
        _contribute = contribute;
    }

    [HttpPost]
    public async Task<ActionResult<CreateGoalResult>> Create(CreateGoalCommand command) =>
        Ok(await _create.HandleAsync(command));

    public record ContributeRequest(decimal Amount);

    [HttpPost("{goalId:guid}/contribute")]
    public async Task<ActionResult<ContributeToGoalResult>> Contribute(Guid goalId, ContributeRequest request) =>
        Ok(await _contribute.HandleAsync(new ContributeToGoalCommand(goalId, request.Amount)));
}
