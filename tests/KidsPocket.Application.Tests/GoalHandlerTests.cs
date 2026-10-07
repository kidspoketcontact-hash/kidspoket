using KidsPocket.Application.Features.Decision.AllocateMoney;
using KidsPocket.Application.Features.Goal.ContributeToGoal;
using KidsPocket.Application.Features.Goal.CreateGoal;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Xunit;

namespace KidsPocket.Application.Tests;

public class GoalHandlerTests
{
    // מכניס 100 ש"ח לקופת Save הפתוחה של הילד, כדי שיהיה ממה לתרום למטרה
    private static async Task<(KidsPocketDbContext Db, Child Child)> ArrangeChildWithOpenSaveBalanceAsync(decimal saveAmount = 100m)
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var moneyEvent = MoneyEvent.Create(child.Id, MoneyEventSource.WeeklyAllowance, saveAmount);
        db.MoneyEvents.Add(moneyEvent);
        var decision = Decision.CreateForMoneyEvent(moneyEvent.Id, child.Id);
        db.Decisions.Add(decision);
        await db.SaveChangesAsync();

        await new AllocateMoneyHandler(db).HandleAsync(
            new AllocateMoneyCommand(decision.Id, new[] { new BucketAllocationInput(BucketIds.Save, saveAmount) }));

        return (db, child);
    }

    [Fact]
    public async Task Contribute_BelowTarget_LeavesGoalIncomplete()
    {
        var (db, child) = await ArrangeChildWithOpenSaveBalanceAsync(100m);
        var goal = await new CreateGoalHandler(db).HandleAsync(new CreateGoalCommand(child.Id, "אופניים", 50m, null));

        var result = await new ContributeToGoalHandler(db).HandleAsync(new ContributeToGoalCommand(goal.GoalId, 30m));

        Assert.Equal(30m, result.CurrentAmount);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task Contribute_ReachingTarget_MarksGoalCompleted()
    {
        var (db, child) = await ArrangeChildWithOpenSaveBalanceAsync(100m);
        var goal = await new CreateGoalHandler(db).HandleAsync(new CreateGoalCommand(child.Id, "אופניים", 50m, null));
        await new ContributeToGoalHandler(db).HandleAsync(new ContributeToGoalCommand(goal.GoalId, 30m));

        var result = await new ContributeToGoalHandler(db).HandleAsync(new ContributeToGoalCommand(goal.GoalId, 20m));

        Assert.Equal(50m, result.CurrentAmount);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public async Task Contribute_MoreThanOpenSaveBalance_ThrowsDomainException()
    {
        var (db, child) = await ArrangeChildWithOpenSaveBalanceAsync(20m);
        var goal = await new CreateGoalHandler(db).HandleAsync(new CreateGoalCommand(child.Id, "אופניים", 50m, null));

        await Assert.ThrowsAsync<DomainException>(() =>
            new ContributeToGoalHandler(db).HandleAsync(new ContributeToGoalCommand(goal.GoalId, 30m)));
    }

    [Fact]
    public async Task Contribute_ToUnknownGoal_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var handler = new ContributeToGoalHandler(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new ContributeToGoalCommand(Guid.NewGuid(), 10m)));
    }
}
