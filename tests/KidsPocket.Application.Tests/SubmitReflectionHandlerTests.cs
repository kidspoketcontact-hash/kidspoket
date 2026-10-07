using KidsPocket.Application.Features.Decision.AllocateMoney;
using KidsPocket.Application.Features.Reflection.SubmitReflection;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Xunit;

namespace KidsPocket.Application.Tests;

public class SubmitReflectionHandlerTests
{
    private static async Task<(KidsPocketDbContext Db, Decision Decision)> ArrangeConfirmedDecisionWithReflectionAsync()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var moneyEvent = MoneyEvent.Create(child.Id, MoneyEventSource.WeeklyAllowance, 100m);
        db.MoneyEvents.Add(moneyEvent);
        var decision = Decision.CreateForMoneyEvent(moneyEvent.Id, child.Id);
        db.Decisions.Add(decision);
        await db.SaveChangesAsync();

        // אישור ההחלטה הוא מה שמתזמן את הרפלקציה (ScheduleReflection נקרא בתוך AllocateMoneyHandler)
        await new AllocateMoneyHandler(db).HandleAsync(new AllocateMoneyCommand(decision.Id, new[]
        {
            new BucketAllocationInput(BucketIds.Save, 100m),
        }));

        return (db, decision);
    }

    [Fact]
    public async Task Handle_WithKnownSentimentOnScheduledReflection_AnswersIt()
    {
        var (db, decision) = await ArrangeConfirmedDecisionWithReflectionAsync();
        var handler = new SubmitReflectionHandler(db);

        var result = await handler.HandleAsync(new SubmitReflectionCommand(decision.Id, "Happy"));

        Assert.Equal("Happy", result.Sentiment);
    }

    [Fact]
    public async Task Handle_WithUnknownSentiment_ThrowsDomainException()
    {
        var (db, decision) = await ArrangeConfirmedDecisionWithReflectionAsync();
        var handler = new SubmitReflectionHandler(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new SubmitReflectionCommand(decision.Id, "Ecstatic")));
    }

    [Fact]
    public async Task Handle_WhenAnsweredTwice_ThrowsDomainException()
    {
        var (db, decision) = await ArrangeConfirmedDecisionWithReflectionAsync();
        var handler = new SubmitReflectionHandler(db);
        await handler.HandleAsync(new SubmitReflectionCommand(decision.Id, "Happy"));

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new SubmitReflectionCommand(decision.Id, "Neutral")));
    }

    [Fact]
    public async Task Handle_WhenDecisionHasNoScheduledReflectionYet_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var moneyEvent = MoneyEvent.Create(child.Id, MoneyEventSource.WeeklyAllowance, 100m);
        db.MoneyEvents.Add(moneyEvent);
        var decision = Decision.CreateForMoneyEvent(moneyEvent.Id, child.Id);
        db.Decisions.Add(decision);
        await db.SaveChangesAsync();

        var handler = new SubmitReflectionHandler(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new SubmitReflectionCommand(decision.Id, "Happy")));
    }
}
