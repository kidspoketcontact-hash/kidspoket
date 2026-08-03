using KidsPocket.Application.Abstractions;
using KidsPocket.Infrastructure.Persistence;

namespace KidsPocket.Application.Features.Goal.CreateGoal;

public class CreateGoalHandler : ICommandHandler<CreateGoalCommand, CreateGoalResult>
{
    private readonly KidsPocketDbContext _db;

    public CreateGoalHandler(KidsPocketDbContext db) => _db = db;

    public async Task<CreateGoalResult> HandleAsync(CreateGoalCommand command, CancellationToken ct = default)
    {
        var goal = Domain.Entities.SavingGoal.Create(command.ChildId, command.Name, command.TargetAmount, command.TargetDate);
        _db.SavingGoals.Add(goal);
        await _db.SaveChangesAsync(ct);

        return new CreateGoalResult(goal.Id, goal.Name, goal.TargetAmount, goal.TargetDate);
    }
}
