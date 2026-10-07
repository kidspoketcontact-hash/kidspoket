namespace KidsPocket.Application.Features.Household.GetHouseholdChildren;

public record GetHouseholdChildrenQuery(Guid HouseholdId);

public record ChildSummary(Guid ChildId, string DisplayName);

public record HouseholdChildrenResult(Guid HouseholdId, IReadOnlyList<ChildSummary> Children);
