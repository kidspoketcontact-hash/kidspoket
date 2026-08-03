using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Allowance.ReceiveAllowance;

public class ReceiveAllowanceHandler : ICommandHandler<ReceiveAllowanceCommand, ReceiveAllowanceResult>
{
    private readonly KidsPocketDbContext _db;

    public ReceiveAllowanceHandler(KidsPocketDbContext db) => _db = db;

    public async Task<ReceiveAllowanceResult> HandleAsync(ReceiveAllowanceCommand command, CancellationToken ct = default)
    {
        if (!Enum.TryParse<MoneyEventSource>(command.Source, ignoreCase: true, out var source))
            throw new DomainException($"מקור כסף לא מוכר: {command.Source}");

        var childExists = await _db.Children.AnyAsync(c => c.Id == command.ChildId, ct);
        if (!childExists)
            throw new DomainException("הילד לא נמצא");

        var moneyEvent = MoneyEvent.Create(command.ChildId, source, command.Amount, description: command.Description);
        _db.MoneyEvents.Add(moneyEvent);

        var decision = Domain.Entities.Decision.CreateForMoneyEvent(moneyEvent.Id, command.ChildId);
        _db.Decisions.Add(decision);

        await _db.SaveChangesAsync(ct);

        return new ReceiveAllowanceResult(moneyEvent.Id, decision.Id, moneyEvent.Amount, moneyEvent.CreatedAt);
    }
}
