namespace KidsPocket.Application.Features.Chore.GetChildChores;

public record GetChildChoresQuery(Guid ChildId);

public record ChoreSummary(
    Guid ChoreId, string Title, string? Description, decimal RewardAmount, string Status);

public record ChildChoresResult(Guid ChildId, IReadOnlyList<ChoreSummary> Chores);
