namespace KidsPocket.Application.Features.Chore.CreateChore;

public record CreateChoreCommand(Guid ChildId, Guid CreatedByAdultId, string Title, string? Description, decimal RewardAmount);

public record CreateChoreResult(Guid ChoreId, string Title, decimal RewardAmount, string Status);
