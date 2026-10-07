using KidsPocket.Api.Auth;
using KidsPocket.Application.Features.Goal.ContributeToGoal;
using KidsPocket.Application.Features.Goal.CreateGoal;
using KidsPocket.Application.Features.Goal.GetChildGoals;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class GoalsController : ControllerBase
{
    private readonly CreateGoalHandler _create;
    private readonly ContributeToGoalHandler _contribute;
    private readonly GetChildGoalsHandler _getChildGoals;
    private readonly KidsPocketDbContext _db;

    public GoalsController(
        CreateGoalHandler create, ContributeToGoalHandler contribute, GetChildGoalsHandler getChildGoals, KidsPocketDbContext db)
    {
        _create = create;
        _contribute = contribute;
        _getChildGoals = getChildGoals;
        _db = db;
    }

    [HttpPost("goals")]
    public async Task<ActionResult<CreateGoalResult>> Create(CreateGoalCommand command)
    {
        if (!await User.CanAccessChildAsync(command.ChildId, _db))
            return Forbid();

        return Ok(await _create.HandleAsync(command));
    }

    public record ContributeRequest(decimal Amount);

    [HttpPost("goals/{goalId:guid}/contribute")]
    public async Task<ActionResult<ContributeToGoalResult>> Contribute(Guid goalId, ContributeRequest request)
    {
        var childId = await _db.SavingGoals.Where(g => g.Id == goalId).Select(g => (Guid?)g.ChildId).SingleOrDefaultAsync();
        if (childId is null || !await User.CanAccessChildAsync(childId.Value, _db))
            return Forbid();

        return Ok(await _contribute.HandleAsync(new ContributeToGoalCommand(goalId, request.Amount)));
    }

    [HttpGet("children/{childId:guid}/goals")]
    public async Task<ActionResult<ChildGoalsResult>> GetChildGoals(Guid childId)
    {
        if (!await User.CanAccessChildAsync(childId, _db))
            return Forbid();

        return Ok(await _getChildGoals.HandleAsync(new GetChildGoalsQuery(childId)));
    }
}
