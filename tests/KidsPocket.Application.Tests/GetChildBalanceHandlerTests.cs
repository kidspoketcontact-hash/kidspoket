using KidsPocket.Application.Features.Decision.AllocateMoney;
using KidsPocket.Application.Features.Ledger.GetChildBalance;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Xunit;

namespace KidsPocket.Application.Tests;

public class GetChildBalanceHandlerTests
{
    [Fact]
    public async Task Handle_ForBrandNewChild_ReturnsZeroForAllThreeDefaultBuckets()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var handler = new GetChildBalanceHandler(db);

        var result = await handler.HandleAsync(new GetChildBalanceQuery(child.Id));

        Assert.Equal(0m, result.TotalBalance);
        Assert.Equal(3, result.Buckets.Count);
        Assert.All(result.Buckets, b => Assert.Equal(0m, b.Balance));
    }

    [Fact]
    public async Task Handle_AfterAllocation_ReflectsLedgerEntriesExactly()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var moneyEvent = MoneyEvent.Create(child.Id, MoneyEventSource.WeeklyAllowance, 100m);
        db.MoneyEvents.Add(moneyEvent);
        var decision = Decision.CreateForMoneyEvent(moneyEvent.Id, child.Id);
        db.Decisions.Add(decision);
        await db.SaveChangesAsync();

        await new AllocateMoneyHandler(db).HandleAsync(new AllocateMoneyCommand(decision.Id, new[]
        {
            new BucketAllocationInput(BucketIds.Enjoy, 10m),
            new BucketAllocationInput(BucketIds.Save, 80m),
            new BucketAllocationInput(BucketIds.Grow, 10m),
        }));

        var result = await new GetChildBalanceHandler(db).HandleAsync(new GetChildBalanceQuery(child.Id));

        Assert.Equal(100m, result.TotalBalance);
        Assert.Equal(10m, result.Buckets.Single(b => b.BucketId == BucketIds.Enjoy).Balance);
        Assert.Equal(80m, result.Buckets.Single(b => b.BucketId == BucketIds.Save).Balance);
        Assert.Equal(10m, result.Buckets.Single(b => b.BucketId == BucketIds.Grow).Balance);
    }

    [Fact]
    public async Task Handle_WhenChildHasNoLedger_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var handler = new GetChildBalanceHandler(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new GetChildBalanceQuery(Guid.NewGuid())));
    }
}
