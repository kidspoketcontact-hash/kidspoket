using KidsPocket.Application.Abstractions;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Chore.GetChildChores;

public class GetChildChoresHandler : IQueryHandler<GetChildChoresQuery, ChildChoresResult>
{
    private readonly KidsPocketDbContext _db;

    public GetChildChoresHandler(KidsPocketDbContext db) => _db = db;

    public async Task<ChildChoresResult> HandleAsync(GetChildChoresQuery query, CancellationToken ct = default)
    {
        var chores = await _db.Chores
            .Where(c => c.ChildId == query.ChildId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChoreSummary(c.Id, c.Title, c.Description, c.RewardAmount, c.Status.ToString()))
            .ToListAsync(ct);

        return new ChildChoresResult(query.ChildId, chores);
    }
}
