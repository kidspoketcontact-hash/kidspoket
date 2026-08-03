using KidsPocket.Application.Features.Decision.AllocateMoney;
using KidsPocket.Application.Features.Decision.GetPendingDecision;
using KidsPocket.Application.Features.Reflection.SubmitReflection;
using Microsoft.AspNetCore.Mvc;

namespace KidsPocket.Api.Controllers;

[ApiController]
[Route("api/decisions")]
public class DecisionsController : ControllerBase
{
    private readonly GetPendingDecisionHandler _getPending;
    private readonly AllocateMoneyHandler _allocate;
    private readonly SubmitReflectionHandler _reflect;

    public DecisionsController(GetPendingDecisionHandler getPending, AllocateMoneyHandler allocate, SubmitReflectionHandler reflect)
    {
        _getPending = getPending;
        _allocate = allocate;
        _reflect = reflect;
    }

    [HttpGet("{decisionId:guid}/pending")]
    public async Task<ActionResult<PendingDecisionResult>> GetPending(Guid decisionId) =>
        Ok(await _getPending.HandleAsync(new GetPendingDecisionQuery(decisionId)));

    public record AllocateRequest(IReadOnlyList<BucketAllocationInput> Allocations);

    [HttpPost("{decisionId:guid}/allocate")]
    public async Task<ActionResult<AllocateMoneyResult>> Allocate(Guid decisionId, AllocateRequest request) =>
        Ok(await _allocate.HandleAsync(new AllocateMoneyCommand(decisionId, request.Allocations)));

    public record ReflectionRequest(string Sentiment);

    [HttpPost("{decisionId:guid}/reflection")]
    public async Task<ActionResult<SubmitReflectionResult>> SubmitReflection(Guid decisionId, ReflectionRequest request) =>
        Ok(await _reflect.HandleAsync(new SubmitReflectionCommand(decisionId, request.Sentiment)));
}
