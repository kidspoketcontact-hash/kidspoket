namespace KidsPocket.Contracts.Decision;

public record BucketSuggestionDto(Guid BucketId, string BucketCode, string BucketName, decimal SuggestedAmount);

public record PendingDecisionResponse(
    Guid DecisionId, Guid MoneyEventId, decimal TotalAmount,
    IReadOnlyList<BucketSuggestionDto> InitialSuggestion, decimal UnallocatedRemainder);

public record BucketAllocationInput(Guid BucketId, decimal Amount);

public record AllocateMoneyRequest(IReadOnlyList<BucketAllocationInput> Allocations);

public record DecisionResponse(Guid Id, string Status, DateTime? ConfirmedAt, IReadOnlyList<BucketAllocationInput> FinalAllocations);
