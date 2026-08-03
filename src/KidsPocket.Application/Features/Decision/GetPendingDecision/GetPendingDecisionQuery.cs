namespace KidsPocket.Application.Features.Decision.GetPendingDecision;

public record GetPendingDecisionQuery(Guid DecisionId);

public record BucketSuggestion(Guid BucketId, string BucketCode, string BucketName, decimal SuggestedAmount);

public record PendingDecisionResult(
    Guid DecisionId, Guid MoneyEventId, decimal TotalAmount,
    IReadOnlyList<BucketSuggestion> InitialSuggestion, decimal UnallocatedRemainder);
