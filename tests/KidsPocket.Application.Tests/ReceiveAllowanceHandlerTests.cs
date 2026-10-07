using KidsPocket.Application.Features.Allowance.ReceiveAllowance;
using KidsPocket.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KidsPocket.Application.Tests;

public class ReceiveAllowanceHandlerTests
{
    [Fact]
    public async Task Handle_WithKnownSourceAndExistingChild_CreatesMoneyEventAndPendingDecision()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var handler = new ReceiveAllowanceHandler(db);

        var result = await handler.HandleAsync(new ReceiveAllowanceCommand(child.Id, "WeeklyAllowance", 100m, "דמי כיס"));

        Assert.Equal(100m, result.Amount);
        var decision = await db.Decisions.SingleAsync(d => d.Id == result.DecisionId);
        Assert.Equal("Pending", decision.Status.ToString());
        Assert.Equal(child.Id, decision.ChildId);
    }

    [Fact]
    public async Task Handle_WithUnknownSource_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var handler = new ReceiveAllowanceHandler(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new ReceiveAllowanceCommand(child.Id, "NotARealSource", 100m, null)));
    }

    [Fact]
    public async Task Handle_WithNonExistentChild_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var handler = new ReceiveAllowanceHandler(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new ReceiveAllowanceCommand(Guid.NewGuid(), "WeeklyAllowance", 100m, null)));
    }
}
