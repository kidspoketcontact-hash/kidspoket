namespace KidsPocket.Contracts.Goals;

public record CreateGoalRequest(Guid ChildId, string Name, decimal TargetAmount, DateTime? TargetDate);

public record ContributeToGoalRequest(Guid GoalId, decimal Amount);

public record GoalResponse(Guid Id, string Name, decimal TargetAmount, decimal CurrentAmount, bool IsCompleted, DateTime? TargetDate);
