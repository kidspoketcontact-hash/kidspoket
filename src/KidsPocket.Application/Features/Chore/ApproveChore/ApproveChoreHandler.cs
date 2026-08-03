using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Chore.ApproveChore;

public class ApproveChoreHandler : ICommandHandler<ApproveChoreCommand, ApproveChoreResult>
{
    private readonly KidsPocketDbContext _db;

    public ApproveChoreHandler(KidsPocketDbContext db) => _db = db;

    public async Task<ApproveChoreResult> HandleAsync(ApproveChoreCommand command, CancellationToken ct = default)
    {
        var chore = await _db.Chores.SingleOrDefaultAsync(c => c.Id == command.ChoreId, ct)
            ?? throw new DomainException("משימה לא נמצאה");

        chore.Approve();

        var moneyEvent = MoneyEvent.Create(
            chore.ChildId, MoneyEventSource.Chore, chore.RewardAmount, chore.CreatedByAdultId, chore.Id, chore.Title);
        _db.MoneyEvents.Add(moneyEvent);

        var decision = Domain.Entities.Decision.CreateForMoneyEvent(moneyEvent.Id, chore.ChildId);
        _db.Decisions.Add(decision);

        await _db.SaveChangesAsync(ct);

        return new ApproveChoreResult(chore.Id, moneyEvent.Id, decision.Id, moneyEvent.Amount);
    }
}
