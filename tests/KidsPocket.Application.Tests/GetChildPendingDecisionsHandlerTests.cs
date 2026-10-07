using KidsPocket.Application.Features.Decision.AllocateMoney;
using KidsPocket.Application.Features.Decision.GetChildPendingDecisions;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Infrastructure.Persistence;
using Xunit;

namespace KidsPocket.Application.Tests;

public class GetChildPendingDecisionsHandlerTests
{
    [Fact]
    public async Task Handle_ListsOnlyPendingDecisionsNotConfirmedOnes()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);

        var pendingEvent = MoneyEvent.Create(child.Id, MoneyEventSource.WeeklyAllowance, 50m);
        db.MoneyEvents.Add(pendingEvent);
        var pendingDecision = Decision.CreateForMoneyEvent(pendingEvent.Id, child.Id);
        db.Decisions.Add(pendingDecision);

        var confirmedEvent = MoneyEvent.Create(child.Id, MoneyEventSource.ManualReward, 20m);
        db.MoneyEvents.Add(confirmedEvent);
        var confirmedDecision = Decision.CreateForMoneyEvent(confirmedEvent.Id, child.Id);
        db.Decisions.Add(confirmedDecision);
        await db.SaveChangesAsync();
        await new AllocateMoneyHandler(db).HandleAsync(
            new AllocateMoneyCommand(confirmedDecision.Id, new[] { new BucketAllocationInput(BucketIds.Save, 20m) }));

        var result = await new GetChildPendingDecisionsHandler(db).HandleAsync(new GetChildPendingDecisionsQuery(child.Id));

        var summary = Assert.Single(result.Decisions);
        Assert.Equal(pendingDecision.Id, summary.DecisionId);
        Assert.Equal(50m, summary.Amount);
        Assert.Equal("WeeklyAllowance", summary.Source);
    }

    [Fact]
    public async Task Handle_ForChildWithNoMoneyEvents_ReturnsEmptyList()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);

        var result = await new GetChildPendingDecisionsHandler(db).HandleAsync(new GetChildPendingDecisionsQuery(child.Id));

        Assert.Empty(result.Decisions);
    }
}
