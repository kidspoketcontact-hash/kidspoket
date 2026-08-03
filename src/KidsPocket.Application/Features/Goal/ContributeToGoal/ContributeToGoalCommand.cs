namespace KidsPocket.Application.Features.Goal.ContributeToGoal;

public record ContributeToGoalCommand(Guid GoalId, decimal Amount);

public record ContributeToGoalResult(Guid GoalId, decimal CurrentAmount, decimal TargetAmount, bool IsCompleted);
