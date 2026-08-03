namespace KidsPocket.Contracts.Ledger;

public record BucketBalanceDto(Guid BucketId, string BucketCode, string BucketName, decimal Balance);

public record ChildBalanceResponse(Guid ChildId, decimal TotalBalance, IReadOnlyList<BucketBalanceDto> Buckets);

public record LedgerEntryResponse(Guid Id, string BucketCode, decimal Amount, string Type, string? Description, DateTime CreatedAt);
