namespace KidsPocket.Application.Features.Ledger.GetChildBalance;

public record GetChildBalanceQuery(Guid ChildId);

public record BucketBalance(Guid BucketId, string BucketCode, string BucketName, decimal Balance);

public record ChildBalanceResult(Guid ChildId, decimal TotalBalance, IReadOnlyList<BucketBalance> Buckets);
