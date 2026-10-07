using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Decision.AllocateMoney;

public class AllocateMoneyHandler : ICommandHandler<AllocateMoneyCommand, AllocateMoneyResult>
{
    private readonly KidsPocketDbContext _db;

    public AllocateMoneyHandler(KidsPocketDbContext db) => _db = db;

    public async Task<AllocateMoneyResult> HandleAsync(AllocateMoneyCommand command, CancellationToken ct = default)
    {
        var decision = await _db.Decisions
            .Include(d => d.MoneyEvent)
            .SingleOrDefaultAsync(d => d.Id == command.DecisionId, ct)
            ?? throw new DomainException("החלטה לא נמצאה");

        var ledger = await _db.Ledgers.SingleOrDefaultAsync(l => l.ChildId == decision.ChildId, ct)
            ?? throw new DomainException("לא נמצא ארנק לילד הזה");

        var allocationDict = command.Allocations.ToDictionary(a => a.BucketId, a => a.Amount);

        // הכלל העסקי היחיד נאכף בתוך ה-Domain: הסכום חייב להתאים בדיוק לסכום שהתקבל
        decision.Confirm(allocationDict, decision.MoneyEvent!.Amount);
        // ה-Id של DecisionAllocation נוצר באפליקציה (לא ב-DB), אז EF לא יכול להבין מתוך
        // graph fixup בלבד שמדובר בישות חדשה - צריך Add מפורש כדי לקבל INSERT ולא UPDATE-שלא-קיים.
        _db.DecisionAllocations.AddRange(decision.Allocations);

        foreach (var allocation in command.Allocations.Where(a => a.Amount != 0))
        {
            var entry = LedgerEntry.Create(
                ledger.Id, allocation.BucketId, allocation.Amount, LedgerEntryType.DecisionAllocation,
                decisionId: decision.Id,
                description: "הקצאת החלטה");
            _db.LedgerEntries.Add(entry);
        }

        // מתזמן תזכורת רפלקציה כמה ימים קדימה - חלק מ-Learning Journey
        decision.ScheduleReflection(DateTime.UtcNow.AddDays(3));
        _db.DecisionReflections.Add(decision.Reflection!);

        await _db.SaveChangesAsync(ct);

        return new AllocateMoneyResult(decision.Id, decision.Status.ToString(), decision.ConfirmedAt);
    }
}
