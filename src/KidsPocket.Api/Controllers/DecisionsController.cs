using KidsPocket.Api.Auth;
using KidsPocket.Application.Features.Decision.AllocateMoney;
using KidsPocket.Application.Features.Decision.GetChildPendingDecisions;
using KidsPocket.Application.Features.Decision.GetPendingDecision;
using KidsPocket.Application.Features.Reflection.SubmitReflection;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class DecisionsController : ControllerBase
{
    private readonly GetPendingDecisionHandler _getPending;
    private readonly AllocateMoneyHandler _allocate;
    private readonly SubmitReflectionHandler _reflect;
    private readonly GetChildPendingDecisionsHandler _getChildPending;
    private readonly KidsPocketDbContext _db;

    public DecisionsController(
        GetPendingDecisionHandler getPending, AllocateMoneyHandler allocate, SubmitReflectionHandler reflect,
        GetChildPendingDecisionsHandler getChildPending, KidsPocketDbContext db)
    {
        _getPending = getPending;
        _allocate = allocate;
        _reflect = reflect;
        _getChildPending = getChildPending;
        _db = db;
    }

    [HttpGet("children/{childId:guid}/decisions/pending")]
    public async Task<ActionResult<ChildPendingDecisionsResult>> GetChildPending(Guid childId)
    {
        if (!await User.CanAccessChildAsync(childId, _db))
            return Forbid();

        return Ok(await _getChildPending.HandleAsync(new GetChildPendingDecisionsQuery(childId)));
    }

    [HttpGet("decisions/{decisionId:guid}/pending")]
    public async Task<ActionResult<PendingDecisionResult>> GetPending(Guid decisionId)
    {
        if (!await CanAccessDecisionAsync(decisionId))
            return Forbid();

        return Ok(await _getPending.HandleAsync(new GetPendingDecisionQuery(decisionId)));
    }

    public record AllocateRequest(IReadOnlyList<BucketAllocationInput> Allocations);

    // ההקצאה עצמה - הבחירה איך לחלק - היא תמיד החלטה של הילד
    [HttpPost("decisions/{decisionId:guid}/allocate")]
    [Authorize(Roles = AppRoles.Child)]
    public async Task<ActionResult<AllocateMoneyResult>> Allocate(Guid decisionId, AllocateRequest request)
    {
        if (!await CanAccessDecisionAsync(decisionId))
            return Forbid();

        return Ok(await _allocate.HandleAsync(new AllocateMoneyCommand(decisionId, request.Allocations)));
    }

    public record ReflectionRequest(string Sentiment);

    [HttpPost("decisions/{decisionId:guid}/reflection")]
    [Authorize(Roles = AppRoles.Child)]
    public async Task<ActionResult<SubmitReflectionResult>> SubmitReflection(Guid decisionId, ReflectionRequest request)
    {
        if (!await CanAccessDecisionAsync(decisionId))
            return Forbid();

        return Ok(await _reflect.HandleAsync(new SubmitReflectionCommand(decisionId, request.Sentiment)));
    }

    private async Task<bool> CanAccessDecisionAsync(Guid decisionId)
    {
        var childId = await _db.Decisions.Where(d => d.Id == decisionId).Select(d => (Guid?)d.ChildId).SingleOrDefaultAsync();
        return childId is not null && await User.CanAccessChildAsync(childId.Value, _db);
    }
}
