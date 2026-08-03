using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Domain.Services;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Goal.ContributeToGoal;

// הפקדה למטרה "נועלת" חלק מקופת ה-Save עבורה - נרשמת כערך שלילי מול Save
// כדי שיתרת ה-Save הפתוחה תשקף רק כסף שעדיין לא יועד לשום מטרה.
public class ContributeToGoalHandler : ICommandHandler<ContributeToGoalCommand, ContributeToGoalResult>
{
    private readonly KidsPocketDbContext _db;

    public ContributeToGoalHandler(KidsPocketDbContext db) => _db = db;

    public async Task<ContributeToGoalResult> HandleAsync(ContributeToGoalCommand command, CancellationToken ct = default)
    {
        var goal = await _db.SavingGoals.SingleOrDefaultAsync(g => g.Id == command.GoalId, ct)
            ?? throw new DomainException("מטרת חיסכון לא נמצאה");

        var ledger = await _db.Ledgers.SingleOrDefaultAsync(l => l.ChildId == goal.ChildId, ct)
            ?? throw new DomainException("לא נמצא ארנק לילד הזה");

        var entries = await _db.LedgerEntries.Where(e => e.LedgerId == ledger.Id).ToListAsync(ct);
        var saveBalance = LedgerCalculator.CalculateBucketBalance(entries, Infrastructure.Persistence.BucketIds.Save);

        if (saveBalance < command.Amount)
            throw new DomainException("אין מספיק יתרה פתוחה בקופת Save");

        var entry = LedgerEntry.Create(
            ledger.Id, Infrastructure.Persistence.BucketIds.Save, -command.Amount, LedgerEntryType.GoalContribution,
            savingGoalId: goal.Id, description: $"הפקדה למטרה: {goal.Name}");
        _db.LedgerEntries.Add(entry);

        var newProgress = LedgerCalculator.CalculateGoalProgress(entries.Append(entry), goal.Id);
        goal.RefreshCompletionStatus(newProgress);

        await _db.SaveChangesAsync(ct);

        return new ContributeToGoalResult(goal.Id, newProgress, goal.TargetAmount, goal.IsCompleted);
    }
}
