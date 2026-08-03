using KidsPocket.Application.Features.Chore.ApproveChore;
using KidsPocket.Application.Features.Chore.CompleteChore;
using KidsPocket.Application.Features.Chore.CreateChore;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/chores")]
public class ChoresController : ControllerBase
{
    private readonly CreateChoreHandler _create;
    private readonly CompleteChoreHandler _complete;
    private readonly ApproveChoreHandler _approve;

    public ChoresController(CreateChoreHandler create, CompleteChoreHandler complete, ApproveChoreHandler approve)
    {
        _create = create;
        _complete = complete;
        _approve = approve;
    }

    [HttpPost]
    public async Task<ActionResult<CreateChoreResult>> Create(CreateChoreCommand command) =>
        Ok(await _create.HandleAsync(command));

    [HttpPost("{choreId:guid}/complete")]
    public async Task<ActionResult<CompleteChoreResult>> Complete(Guid choreId) =>
        Ok(await _complete.HandleAsync(new CompleteChoreCommand(choreId)));

    [HttpPost("{choreId:guid}/approve")]
    public async Task<ActionResult<ApproveChoreResult>> Approve(Guid choreId) =>
        Ok(await _approve.HandleAsync(new ApproveChoreCommand(choreId)));
}
