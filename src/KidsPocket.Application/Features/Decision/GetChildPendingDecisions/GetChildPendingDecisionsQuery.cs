namespace KidsPocket.Application.Features.Decision.GetChildPendingDecisions;

public record GetChildPendingDecisionsQuery(Guid ChildId);

public record PendingDecisionSummary(Guid DecisionId, decimal Amount, string Source, DateTime CreatedAt);

public record ChildPendingDecisionsResult(Guid ChildId, IReadOnlyList<PendingDecisionSummary> Decisions);
