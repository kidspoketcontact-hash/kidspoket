namespace KidsPocket.Application.Features.Goal.GetChildGoals;

public record GetChildGoalsQuery(Guid ChildId);

public record GoalSummary(Guid GoalId, string Name, decimal TargetAmount, decimal CurrentAmount, bool IsCompleted, DateTime? TargetDate);

public record ChildGoalsResult(Guid ChildId, IReadOnlyList<GoalSummary> Goals);
