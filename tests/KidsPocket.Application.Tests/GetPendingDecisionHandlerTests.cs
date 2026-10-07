using KidsPocket.Application.Features.Decision.GetPendingDecision;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Xunit;

namespace KidsPocket.Application.Tests;

public class GetPendingDecisionHandlerTests
{
    [Fact]
    public async Task Handle_ForHundredShekelMoneyEvent_SuggestsTenPercentPerDefaultBucketWithRemainder()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var moneyEvent = MoneyEvent.Create(child.Id, MoneyEventSource.WeeklyAllowance, 100m);
        db.MoneyEvents.Add(moneyEvent);
        var decision = Decision.CreateForMoneyEvent(moneyEvent.Id, child.Id);
        db.Decisions.Add(decision);
        await db.SaveChangesAsync();

        var handler = new GetPendingDecisionHandler(db);
        var result = await handler.HandleAsync(new GetPendingDecisionQuery(decision.Id));

        Assert.Equal(100m, result.TotalAmount);
        Assert.Equal(70m, result.UnallocatedRemainder);
        Assert.Equal(3, result.InitialSuggestion.Count);
        Assert.All(result.InitialSuggestion, s => Assert.Equal(10m, s.SuggestedAmount));
    }

    [Fact]
    public async Task Handle_WhenDecisionDoesNotExist_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var handler = new GetPendingDecisionHandler(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new GetPendingDecisionQuery(Guid.NewGuid())));
    }
}
