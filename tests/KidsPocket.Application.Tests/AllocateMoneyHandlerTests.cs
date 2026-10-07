using KidsPocket.Application.Features.Decision.AllocateMoney;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KidsPocket.Application.Tests;

public class AllocateMoneyHandlerTests
{
    private static async Task<(KidsPocketDbContext Db, Child Child, Decision Decision)> ArrangePendingDecisionAsync(decimal amount = 100m)
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);

        var moneyEvent = MoneyEvent.Create(child.Id, MoneyEventSource.WeeklyAllowance, amount);
        db.MoneyEvents.Add(moneyEvent);
        var decision = Domain.Entities.Decision.CreateForMoneyEvent(moneyEvent.Id, child.Id);
        db.Decisions.Add(decision);
        await db.SaveChangesAsync();

        return (db, child, decision);
    }

    // רגרסיה לבאג שנתפס בהרצה ידנית: DecisionAllocation/DecisionReflection נוצרים עם Id מיוצר
    // באפליקציה ומגיעים ל-Change Tracker רק דרך navigation fixup, מה שגרם ל-EF לחשוב שהם
    // ישויות קיימות וליצור UPDATE במקום INSERT -> DbUpdateConcurrencyException (0 שורות הושפעו).
    [Fact]
    public async Task Allocate_WithValidAllocationsSummingToTotal_DoesNotThrowConcurrencyException()
    {
        var (db, _, decision) = await ArrangePendingDecisionAsync(100m);
        var handler = new AllocateMoneyHandler(db);
        var command = new AllocateMoneyCommand(decision.Id, new[]
        {
            new BucketAllocationInput(BucketIds.Enjoy, 10m),
            new BucketAllocationInput(BucketIds.Save, 80m),
            new BucketAllocationInput(BucketIds.Grow, 10m),
        });

        var result = await handler.HandleAsync(command);

        Assert.Equal("Confirmed", result.Status);
        Assert.NotNull(result.ConfirmedAt);
    }

    [Fact]
    public async Task Allocate_WithValidAllocations_PersistsAllocationsAndLedgerEntriesAndReflection()
    {
        var (db, child, decision) = await ArrangePendingDecisionAsync(100m);
        var handler = new AllocateMoneyHandler(db);
        var command = new AllocateMoneyCommand(decision.Id, new[]
        {
            new BucketAllocationInput(BucketIds.Enjoy, 10m),
            new BucketAllocationInput(BucketIds.Save, 80m),
            new BucketAllocationInput(BucketIds.Grow, 10m),
        });

        await handler.HandleAsync(command);

        var allocations = await db.DecisionAllocations.Where(a => a.DecisionId == decision.Id).ToListAsync();
        Assert.Equal(3, allocations.Count);
        Assert.Equal(100m, allocations.Sum(a => a.Amount));

        var ledger = await db.Ledgers.SingleAsync(l => l.ChildId == child.Id);
        var entries = await db.LedgerEntries.Where(e => e.LedgerId == ledger.Id).ToListAsync();
        Assert.Equal(3, entries.Count);
        Assert.Equal(100m, entries.Sum(e => e.Amount));

        var reflection = await db.DecisionReflections.SingleOrDefaultAsync(r => r.DecisionId == decision.Id);
        Assert.NotNull(reflection);
        Assert.Null(reflection!.AnsweredAt);
    }

    [Fact]
    public async Task Allocate_SkipsZeroAmountBuckets_DoesNotCreateLedgerEntryForThem()
    {
        var (db, _, decision) = await ArrangePendingDecisionAsync(100m);
        var handler = new AllocateMoneyHandler(db);
        var command = new AllocateMoneyCommand(decision.Id, new[]
        {
            new BucketAllocationInput(BucketIds.Enjoy, 0m),
            new BucketAllocationInput(BucketIds.Save, 100m),
            new BucketAllocationInput(BucketIds.Grow, 0m),
        });

        await handler.HandleAsync(command);

        var entries = await db.LedgerEntries.ToListAsync();
        Assert.Single(entries);
        Assert.Equal(BucketIds.Save, entries[0].BucketId);
    }

    [Fact]
    public async Task Allocate_WhenAllocationSumDoesNotMatchMoneyEventAmount_ThrowsDomainException()
    {
        var (db, _, decision) = await ArrangePendingDecisionAsync(100m);
        var handler = new AllocateMoneyHandler(db);
        var command = new AllocateMoneyCommand(decision.Id, new[]
        {
            new BucketAllocationInput(BucketIds.Enjoy, 10m),
            new BucketAllocationInput(BucketIds.Save, 10m),
            new BucketAllocationInput(BucketIds.Grow, 10m),
        });

        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task Allocate_WhenDecisionAlreadyConfirmed_ThrowsDomainException()
    {
        var (db, _, decision) = await ArrangePendingDecisionAsync(100m);
        var handler = new AllocateMoneyHandler(db);
        var command = new AllocateMoneyCommand(decision.Id, new[] { new BucketAllocationInput(BucketIds.Save, 100m) });
        await handler.HandleAsync(command);

        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task Allocate_WhenDecisionDoesNotExist_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var handler = new AllocateMoneyHandler(db);
        var command = new AllocateMoneyCommand(Guid.NewGuid(), new[] { new BucketAllocationInput(BucketIds.Save, 100m) });

        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
    }
}
