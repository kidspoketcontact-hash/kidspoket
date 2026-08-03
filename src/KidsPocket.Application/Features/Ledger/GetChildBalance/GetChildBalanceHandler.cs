using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Domain.Services;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Ledger.GetChildBalance;

// היתרה לעולם לא מאוחסנת - תמיד מחושבת מחדש מכל רשומות היומן
public class GetChildBalanceHandler : IQueryHandler<GetChildBalanceQuery, ChildBalanceResult>
{
    private readonly KidsPocketDbContext _db;

    public GetChildBalanceHandler(KidsPocketDbContext db) => _db = db;

    public async Task<ChildBalanceResult> HandleAsync(GetChildBalanceQuery query, CancellationToken ct = default)
    {
        var ledger = await _db.Ledgers.SingleOrDefaultAsync(l => l.ChildId == query.ChildId, ct)
            ?? throw new DomainException("לא נמצא ארנק לילד הזה");

        var entries = await _db.LedgerEntries.Where(e => e.LedgerId == ledger.Id).ToListAsync(ct);
        var buckets = await _db.Buckets.OrderBy(b => b.SortOrder).ToListAsync(ct);

        var balancesByBucket = LedgerCalculator.CalculateAllBucketBalances(entries);

        var bucketBalances = buckets
            .Select(b => new BucketBalance(b.Id, b.Code, b.DisplayName, balancesByBucket.GetValueOrDefault(b.Id)))
            .Where(b => b.Balance != 0 || buckets.First(x => x.Id == b.BucketId).IsSystemDefault)
            .ToList();

        var total = LedgerCalculator.CalculateTotalBalance(entries);

        return new ChildBalanceResult(query.ChildId, total, bucketBalances);
    }
}
