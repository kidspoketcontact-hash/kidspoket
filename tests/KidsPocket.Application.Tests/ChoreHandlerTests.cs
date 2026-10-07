using KidsPocket.Application.Features.Chore.ApproveChore;
using KidsPocket.Application.Features.Chore.CompleteChore;
using KidsPocket.Application.Features.Chore.CreateChore;
using KidsPocket.Domain.Exceptions;
using Xunit;

namespace KidsPocket.Application.Tests;

public class ChoreHandlerTests
{
    [Fact]
    public async Task FullLifecycle_CreateCompleteApprove_EndsWithMoneyEventAndPendingDecision()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var adultId = Guid.NewGuid();

        var created = await new CreateChoreHandler(db).HandleAsync(
            new CreateChoreCommand(child.Id, adultId, "לסדר את החדר", "כולל מיטה ושולחן", 15m));
        Assert.Equal("Pending", created.Status);

        var completed = await new CompleteChoreHandler(db).HandleAsync(new CompleteChoreCommand(created.ChoreId));
        Assert.Equal("Completed", completed.Status);

        var approved = await new ApproveChoreHandler(db).HandleAsync(new ApproveChoreCommand(created.ChoreId));
        Assert.Equal(15m, approved.Amount);
        Assert.NotEqual(Guid.Empty, approved.MoneyEventId);
        Assert.NotEqual(Guid.Empty, approved.DecisionId);
    }

    [Fact]
    public async Task Approve_BeforeCompleted_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var created = await new CreateChoreHandler(db).HandleAsync(
            new CreateChoreCommand(child.Id, Guid.NewGuid(), "לשטוף כלים", null, 5m));

        await Assert.ThrowsAsync<DomainException>(() =>
            new ApproveChoreHandler(db).HandleAsync(new ApproveChoreCommand(created.ChoreId)));
    }

    [Fact]
    public async Task Complete_Twice_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var created = await new CreateChoreHandler(db).HandleAsync(
            new CreateChoreCommand(child.Id, Guid.NewGuid(), "להוציא אשפה", null, 5m));
        await new CompleteChoreHandler(db).HandleAsync(new CompleteChoreCommand(created.ChoreId));

        await Assert.ThrowsAsync<DomainException>(() =>
            new CompleteChoreHandler(db).HandleAsync(new CompleteChoreCommand(created.ChoreId)));
    }
}
