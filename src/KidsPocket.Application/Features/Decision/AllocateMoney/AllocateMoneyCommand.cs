namespace KidsPocket.Application.Features.Decision.AllocateMoney;

public record BucketAllocationInput(Guid BucketId, decimal Amount);

// הילד שולח את ההקצאה הסופית שלו בין הקופות - כולל הקופות המותאמות אישית אם יש
public record AllocateMoneyCommand(Guid DecisionId, IReadOnlyList<BucketAllocationInput> Allocations);

public record AllocateMoneyResult(Guid DecisionId, string Status, DateTime? ConfirmedAt);
