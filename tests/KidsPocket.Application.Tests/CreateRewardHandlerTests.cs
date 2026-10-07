using KidsPocket.Application.Features.Reward.CreateReward;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace KidsPocket.Application.Tests;

public class CreateRewardHandlerTests
{
    [Fact]
    public async Task Handle_CreatesRewardLinkedToMoneyEventAndPendingDecision()
    {
        var db = TestDb.Create();
        var child = await TestSeed.CreateChildWithLedgerAsync(db);
        var adultId = Guid.NewGuid();
        var handler = new CreateRewardHandler(db);

        var result = await handler.HandleAsync(new CreateRewardCommand(child.Id, adultId, 25m, "כל הכבוד!"));

        var reward = await db.Rewards.SingleAsync(r => r.Id == result.RewardId);
        Assert.Equal(result.MoneyEventId, reward.MoneyEventId);

        var moneyEvent = await db.MoneyEvents.SingleAsync(m => m.Id == result.MoneyEventId);
        Assert.Equal(25m, moneyEvent.Amount);

        var decision = await db.Decisions.SingleAsync(d => d.Id == result.DecisionId);
        Assert.Equal(moneyEvent.Id, decision.MoneyEventId);
        Assert.Equal("Pending", decision.Status.ToString());
    }
}
