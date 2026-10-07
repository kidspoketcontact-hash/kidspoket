using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Household.GetHouseholdChildren;

// לוח הבקרה של ההורה מתחיל כאן - כל הילדים המשויכים לבית האב הזה דרך ChildAdult
// (ילד יכול להיות משויך למספר בתי אב, אז זו טבלת קישור ולא FK ישיר על Child).
public class GetHouseholdChildrenHandler : IQueryHandler<GetHouseholdChildrenQuery, HouseholdChildrenResult>
{
    private readonly KidsPocketDbContext _db;

    public GetHouseholdChildrenHandler(KidsPocketDbContext db) => _db = db;

    public async Task<HouseholdChildrenResult> HandleAsync(GetHouseholdChildrenQuery query, CancellationToken ct = default)
    {
        var householdExists = await _db.Households.AnyAsync(h => h.Id == query.HouseholdId, ct);
        if (!householdExists)
            throw new DomainException("בית אב לא נמצא");

        var children = await _db.ChildAdults
            .Where(ca => ca.HouseholdId == query.HouseholdId)
            .Join(_db.Children, ca => ca.ChildId, c => c.Id, (ca, c) => new ChildSummary(c.Id, c.DisplayName))
            .ToListAsync(ct);

        return new HouseholdChildrenResult(query.HouseholdId, children);
    }
}
