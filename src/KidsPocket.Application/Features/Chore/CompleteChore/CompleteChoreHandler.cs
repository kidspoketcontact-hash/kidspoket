using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Chore.CompleteChore;

public class CompleteChoreHandler : ICommandHandler<CompleteChoreCommand, CompleteChoreResult>
{
    private readonly KidsPocketDbContext _db;

    public CompleteChoreHandler(KidsPocketDbContext db) => _db = db;

    public async Task<CompleteChoreResult> HandleAsync(CompleteChoreCommand command, CancellationToken ct = default)
    {
        var chore = await _db.Chores.SingleOrDefaultAsync(c => c.Id == command.ChoreId, ct)
            ?? throw new DomainException("משימה לא נמצאה");

        chore.MarkCompleted();
        await _db.SaveChangesAsync(ct);

        return new CompleteChoreResult(chore.Id, chore.Status.ToString());
    }
}
