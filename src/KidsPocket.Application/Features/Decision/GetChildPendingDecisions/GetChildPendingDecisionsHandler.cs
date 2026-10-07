using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Enums;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Decision.GetChildPendingDecisions;

// אחרי שה-Auth האמיתי הופרד - הורה יכול ליצור MoneyEvent (דמי כיס/תגמול/אישור משימה) בשביל
// הילד, אבל רק הילד יכול להחליט איך לחלק. הילד צריך דרך לגלות אילו החלטות מחכות לו כשהוא נכנס.
public class GetChildPendingDecisionsHandler : IQueryHandler<GetChildPendingDecisionsQuery, ChildPendingDecisionsResult>
{
    private readonly KidsPocketDbContext _db;

    public GetChildPendingDecisionsHandler(KidsPocketDbContext db) => _db = db;

    public async Task<ChildPendingDecisionsResult> HandleAsync(GetChildPendingDecisionsQuery query, CancellationToken ct = default)
    {
        var decisions = await _db.Decisions
            .Include(d => d.MoneyEvent)
            .Where(d => d.ChildId == query.ChildId && d.Status == DecisionStatus.Pending)
            .OrderBy(d => d.CreatedAt)
            .Select(d => new PendingDecisionSummary(d.Id, d.MoneyEvent!.Amount, d.MoneyEvent!.Source.ToString(), d.CreatedAt))
            .ToListAsync(ct);

        return new ChildPendingDecisionsResult(query.ChildId, decisions);
    }
}
