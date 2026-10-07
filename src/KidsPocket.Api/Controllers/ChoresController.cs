using KidsPocket.Api.Auth;
using KidsPocket.Application.Features.Chore.ApproveChore;
using KidsPocket.Application.Features.Chore.CompleteChore;
using KidsPocket.Application.Features.Chore.CreateChore;
using KidsPocket.Application.Features.Chore.GetChildChores;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ChoresController : ControllerBase
{
    private readonly CreateChoreHandler _create;
    private readonly CompleteChoreHandler _complete;
    private readonly ApproveChoreHandler _approve;
    private readonly GetChildChoresHandler _getChildChores;
    private readonly KidsPocketDbContext _db;

    public ChoresController(
        CreateChoreHandler create, CompleteChoreHandler complete, ApproveChoreHandler approve,
        GetChildChoresHandler getChildChores, KidsPocketDbContext db)
    {
        _create = create;
        _complete = complete;
        _approve = approve;
        _getChildChores = getChildChores;
        _db = db;
    }

    // רק הורה יוצר משימה לילד שלו
    [HttpPost("chores")]
    [Authorize(Roles = AppRoles.Parent)]
    public async Task<ActionResult<CreateChoreResult>> Create(CreateChoreCommand command)
    {
        if (!await User.CanAccessChildAsync(command.ChildId, _db))
            return Forbid();

        return Ok(await _create.HandleAsync(command));
    }

    // הילד עצמו מסמן שביצע
    [HttpPost("chores/{choreId:guid}/complete")]
    [Authorize(Roles = AppRoles.Child)]
    public async Task<ActionResult<CompleteChoreResult>> Complete(Guid choreId)
    {
        if (!await CanAccessChoreAsync(choreId))
            return Forbid();

        return Ok(await _complete.HandleAsync(new CompleteChoreCommand(choreId)));
    }

    // רק הורה מאשר ומזכה בכסף
    [HttpPost("chores/{choreId:guid}/approve")]
    [Authorize(Roles = AppRoles.Parent)]
    public async Task<ActionResult<ApproveChoreResult>> Approve(Guid choreId)
    {
        if (!await CanAccessChoreAsync(choreId))
            return Forbid();

        return Ok(await _approve.HandleAsync(new ApproveChoreCommand(choreId)));
    }

    [HttpGet("children/{childId:guid}/chores")]
    public async Task<ActionResult<ChildChoresResult>> GetChildChores(Guid childId)
    {
        if (!await User.CanAccessChildAsync(childId, _db))
            return Forbid();

        return Ok(await _getChildChores.HandleAsync(new GetChildChoresQuery(childId)));
    }

    private async Task<bool> CanAccessChoreAsync(Guid choreId)
    {
        var childId = await _db.Chores.Where(c => c.Id == choreId).Select(c => (Guid?)c.ChildId).SingleOrDefaultAsync();
        return childId is not null && await User.CanAccessChildAsync(childId.Value, _db);
    }
}
