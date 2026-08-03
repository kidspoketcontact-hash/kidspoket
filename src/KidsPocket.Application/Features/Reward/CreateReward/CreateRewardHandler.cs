using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Infrastructure.Persistence;

namespace KidsPocket.Application.Features.Reward.CreateReward;

public class CreateRewardHandler : ICommandHandler<CreateRewardCommand, CreateRewardResult>
{
    private readonly KidsPocketDbContext _db;

    public CreateRewardHandler(KidsPocketDbContext db) => _db = db;

    public async Task<CreateRewardResult> HandleAsync(CreateRewardCommand command, CancellationToken ct = default)
    {
        var reward = Domain.Entities.Reward.Create(command.ChildId, command.CreatedByAdultId, command.Amount, command.Description);
        _db.Rewards.Add(reward);

        var moneyEvent = MoneyEvent.Create(
            command.ChildId, MoneyEventSource.ManualReward, command.Amount, command.CreatedByAdultId, description: command.Description);
        _db.MoneyEvents.Add(moneyEvent);
        reward.LinkMoneyEvent(moneyEvent.Id);

        var decision = Domain.Entities.Decision.CreateForMoneyEvent(moneyEvent.Id, command.ChildId);
        _db.Decisions.Add(decision);

        await _db.SaveChangesAsync(ct);

        return new CreateRewardResult(reward.Id, moneyEvent.Id, decision.Id);
    }
}
