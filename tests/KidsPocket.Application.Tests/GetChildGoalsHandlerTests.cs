using KidsPocket.Application.Features.Decision.AllocateMoney;
using KidsPocket.Application.Features.Goal.ContributeToGoal;
using KidsPocket.Application.Features.Goal.CreateGoal;
using KidsPocket.Application.Features.Goal.GetChildGoals;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Infrastructure.Persistence;
using Xunit;

namespace KidsPocket.Application.Tests;

public class GetChildGoalsHandlerTests
{
    [Fact]
    public async Task Handle_ForChildWithNoGoals_ReturnsEmptyList()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);

        var result = await new GetChildGoalsHandler(db).HandleAsync(new GetChildGoalsQuery(child.Id));

        Assert.Empty(result.Goals);
    }

    [Fact]
    public async Task Handle_ReflectsContributionsAsCurrentAmount_DerivedFromLedgerNotStoredField()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);

        // מכניסים 100 לקופת Save הפתוחה כדי שיהיה ממה לתרום
        var moneyEvent = MoneyEvent.Create(child.Id, MoneyEventSource.WeeklyAllowance, 100m);
        db.MoneyEvents.Add(moneyEvent);
        var decision = Decision.CreateForMoneyEvent(moneyEvent.Id, child.Id);
        db.Decisions.Add(decision);
        await db.SaveChangesAsync();
        await new AllocateMoneyHandler(db).HandleAsync(
            new AllocateMoneyCommand(decision.Id, new[] { new BucketAllocationInput(BucketIds.Save, 100m) }));

        var goal = await new CreateGoalHandler(db).HandleAsync(new CreateGoalCommand(child.Id, "אופניים", 50m, null));
        await new ContributeToGoalHandler(db).HandleAsync(new ContributeToGoalCommand(goal.GoalId, 30m));

        var result = await new GetChildGoalsHandler(db).HandleAsync(new GetChildGoalsQuery(child.Id));

        var summary = Assert.Single(result.Goals);
        Assert.Equal("אופניים", summary.Name);
        Assert.Equal(30m, summary.CurrentAmount);
        Assert.Equal(50m, summary.TargetAmount);
        Assert.False(summary.IsCompleted);
    }

    [Fact]
    public async Task Handle_DoesNotReturnGoalsBelongingToOtherChildren()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db, "דני");
        var otherChild = await TestSeed.CreateChildWithLedgerAsync(db, "ילד אחר");
        await new CreateGoalHandler(db).HandleAsync(new CreateGoalCommand(otherChild.Id, "מטרה של מישהו אחר", 20m, null));

        var result = await new GetChildGoalsHandler(db).HandleAsync(new GetChildGoalsQuery(child.Id));

        Assert.Empty(result.Goals);
    }
}
