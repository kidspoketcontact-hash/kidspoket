using KidsPocket.Application.Features.Chore.CompleteChore;
using KidsPocket.Application.Features.Chore.CreateChore;
using KidsPocket.Application.Features.Chore.GetChildChores;
using Xunit;

namespace KidsPocket.Application.Tests;

public class GetChildChoresHandlerTests
{
    [Fact]
    public async Task Handle_ForChildWithNoChores_ReturnsEmptyList()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);

        var result = await new GetChildChoresHandler(db).HandleAsync(new GetChildChoresQuery(child.Id));

        Assert.Empty(result.Chores);
    }

    [Fact]
    public async Task Handle_ReflectsStatusChangesAsTheChoreProgresses()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var created = await new CreateChoreHandler(db).HandleAsync(
            new CreateChoreCommand(child.Id, Guid.NewGuid(), "לסדר את החדר", "כולל מיטה", 15m));

        var pending = await new GetChildChoresHandler(db).HandleAsync(new GetChildChoresQuery(child.Id));
        Assert.Equal("Pending", Assert.Single(pending.Chores).Status);

        await new CompleteChoreHandler(db).HandleAsync(new CompleteChoreCommand(created.ChoreId));

        var completed = await new GetChildChoresHandler(db).HandleAsync(new GetChildChoresQuery(child.Id));
        var summary = Assert.Single(completed.Chores);
        Assert.Equal("Completed", summary.Status);
        Assert.Equal(15m, summary.RewardAmount);
        Assert.Equal("לסדר את החדר", summary.Title);
    }

    [Fact]
    public async Task Handle_DoesNotReturnChoresBelongingToOtherChildren()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db, "דני");
        var otherChild = await TestSeed.CreateChildWithLedgerAsync(db, "ילד אחר");
        await new CreateChoreHandler(db).HandleAsync(
            new CreateChoreCommand(otherChild.Id, Guid.NewGuid(), "משימה של מישהו אחר", null, 5m));

        var result = await new GetChildChoresHandler(db).HandleAsync(new GetChildChoresQuery(child.Id));

        Assert.Empty(result.Chores);
    }
}
