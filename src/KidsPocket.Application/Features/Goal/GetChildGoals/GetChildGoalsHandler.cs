using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Services;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Goal.GetChildGoals;

// כמו כל יתרה במערכת - ההתקדמות של מטרה לעולם לא מאוחסנת, תמיד נגזרת מחדש מה-Ledger
public class GetChildGoalsHandler : IQueryHandler<GetChildGoalsQuery, ChildGoalsResult>
{
    private readonly KidsPocketDbContext _db;

    public GetChildGoalsHandler(KidsPocketDbContext db) => _db = db;

    public async Task<ChildGoalsResult> HandleAsync(GetChildGoalsQuery query, CancellationToken ct = default)
    {
        var goals = await _db.SavingGoals.Where(g => g.ChildId == query.ChildId).ToListAsync(ct);
        var goalIds = goals.Select(g => g.Id).ToList();

        var entries = await _db.LedgerEntries
            .Where(e => e.SavingGoalId != null && goalIds.Contains(e.SavingGoalId!.Value))
            .ToListAsync(ct);

        var summaries = goals
            .Select(g => new GoalSummary(
                g.Id, g.Name, g.TargetAmount,
                LedgerCalculator.CalculateGoalProgress(entries, g.Id),
                g.IsCompleted, g.TargetDate))
            .ToList();

        return new ChildGoalsResult(query.ChildId, summaries);
    }
}
