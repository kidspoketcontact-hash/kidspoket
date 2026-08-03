using KidsPocket.Application.Abstractions;
using KidsPocket.Infrastructure.Persistence;

namespace KidsPocket.Application.Features.Chore.CreateChore;

public class CreateChoreHandler : ICommandHandler<CreateChoreCommand, CreateChoreResult>
{
    private readonly KidsPocketDbContext _db;

    public CreateChoreHandler(KidsPocketDbContext db) => _db = db;

    public async Task<CreateChoreResult> HandleAsync(CreateChoreCommand command, CancellationToken ct = default)
    {
        var chore = Domain.Entities.Chore.Create(command.ChildId, command.CreatedByAdultId, command.Title, command.RewardAmount, command.Description);
        _db.Chores.Add(chore);
        await _db.SaveChangesAsync(ct);

        return new CreateChoreResult(chore.Id, chore.Title, chore.RewardAmount, chore.Status.ToString());
    }
}
