using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Domain.Services;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Decision.GetPendingDecision;

// מציג לילד את הצעת ההקצאה הראשונית (10% לכל קופת ברירת מחדל) והשאר הפתוח לבחירה חופשית
public class GetPendingDecisionHandler : IQueryHandler<GetPendingDecisionQuery, PendingDecisionResult>
{
    private readonly KidsPocketDbContext _db;

    public GetPendingDecisionHandler(KidsPocketDbContext db) => _db = db;

    public async Task<PendingDecisionResult> HandleAsync(GetPendingDecisionQuery query, CancellationToken ct = default)
    {
        var decision = await _db.Decisions
            .Include(d => d.MoneyEvent)
            .SingleOrDefaultAsync(d => d.Id == query.DecisionId, ct)
            ?? throw new DomainException("החלטה לא נמצאה");

        var defaultBuckets = await _db.Buckets.Where(b => b.IsSystemDefault).OrderBy(b => b.SortOrder).ToListAsync(ct);

        var suggestionByBucket = DecisionEngine.BuildInitialSuggestion(decision.MoneyEvent!.Amount, defaultBuckets);
        var remainder = DecisionEngine.CalculateUnallocatedRemainder(decision.MoneyEvent.Amount, suggestionByBucket);

        var suggestions = defaultBuckets
            .Select(b => new BucketSuggestion(b.Id, b.Code, b.DisplayName, suggestionByBucket.GetValueOrDefault(b.Id)))
            .ToList();

        return new PendingDecisionResult(decision.Id, decision.MoneyEventId, decision.MoneyEvent.Amount, suggestions, remainder);
    }
}
