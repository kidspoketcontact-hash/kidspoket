namespace KidsPocket.Contracts.Chores;

public record CreateChoreRequest(Guid ChildId, Guid CreatedByAdultId, string Title, string? Description, decimal RewardAmount);

public record ChoreResponse(Guid Id, string Title, string? Description, decimal RewardAmount, string Status);
