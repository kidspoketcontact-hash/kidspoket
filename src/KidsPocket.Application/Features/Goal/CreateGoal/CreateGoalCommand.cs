namespace KidsPocket.Application.Features.Goal.CreateGoal;

public record CreateGoalCommand(Guid ChildId, string Name, decimal TargetAmount, DateTime? TargetDate);

public record CreateGoalResult(Guid GoalId, string Name, decimal TargetAmount, DateTime? TargetDate);
