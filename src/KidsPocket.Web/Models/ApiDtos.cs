namespace KidsPocket.Web.Models;

// שלד ה-Onboarding עדיין לא חשוף דרך KidsPocket.Contracts (הוא Bootstrap גס ב-OnboardingController),
// אז ה-DTOs האלה ממופים ידנית לצורת ה-JSON שהוא מחזיר.
public record CreateHouseholdApiRequest(string Name);
public record CreateHouseholdResult(Guid Id, string Name);

public record CreateChildApiRequest(string DisplayName, string? Pin);
public record CreateChildResult(Guid Id, string DisplayName, Guid LedgerId);

public record AddAdultApiRequest(string DisplayName, string Role, string Email, string Password);
public record AddAdultResultDto(Guid Id, string DisplayName, string Role);

public record ParentLoginApiRequest(string Email, string Password);
public record ParentLoginResultDto(string Token, Guid AdultId, Guid HouseholdId, string DisplayName);

public record ChildLoginApiRequest(Guid ChildId, string Pin);
public record ChildLoginResultDto(string Token, Guid ChildId, string DisplayName);

public record GoogleLoginApiRequest(string IdToken, string? HouseholdName);
public record AppleLoginApiRequest(string IdToken, string? HouseholdName, string? Email, string? DisplayName);
public record ExternalLoginResultDto(string Token, Guid AdultId, Guid HouseholdId, string DisplayName, bool IsNewHousehold);

public record CreateMoneyEventResult(Guid MoneyEventId, Guid DecisionId, decimal Amount, DateTime CreatedAt);

public record AllocateMoneyResultDto(Guid DecisionId, string Status, DateTime? ConfirmedAt);

public record ChildSummaryDto(Guid ChildId, string DisplayName);
public record HouseholdChildrenResult(Guid HouseholdId, List<ChildSummaryDto> Children);

// שוב לא דרך KidsPocket.Contracts - הצורה שם (GoalResponse עם Id/CurrentAmount/IsCompleted גורף)
// לא תואמת בדיוק לצורת ה-JSON שכל אחד מה-endpoints האלה מחזיר בפועל.
public record CreateGoalApiRequest(Guid ChildId, string Name, decimal TargetAmount, DateTime? TargetDate);
public record CreateGoalResultDto(Guid GoalId, string Name, decimal TargetAmount, DateTime? TargetDate);

public record ContributeToGoalApiRequest(decimal Amount);
public record ContributeToGoalResultDto(Guid GoalId, decimal CurrentAmount, decimal TargetAmount, bool IsCompleted);

public record GoalSummaryDto(Guid GoalId, string Name, decimal TargetAmount, decimal CurrentAmount, bool IsCompleted, DateTime? TargetDate);
public record ChildGoalsResult(Guid ChildId, List<GoalSummaryDto> Goals);

public record CreateChoreApiRequest(Guid ChildId, Guid CreatedByAdultId, string Title, string? Description, decimal RewardAmount);
public record CreateChoreResultDto(Guid ChoreId, string Title, decimal RewardAmount, string Status);

public record CompleteChoreResultDto(Guid ChoreId, string Status);
public record ApproveChoreResultDto(Guid ChoreId, Guid MoneyEventId, Guid DecisionId, decimal Amount);

public record ChoreSummaryDto(Guid ChoreId, string Title, string? Description, decimal RewardAmount, string Status);
public record ChildChoresResult(Guid ChildId, List<ChoreSummaryDto> Chores);

public record CreateRewardApiRequest(Guid ChildId, Guid CreatedByAdultId, decimal Amount, string? Description);
public record CreateRewardResultDto(Guid RewardId, Guid MoneyEventId, Guid DecisionId);

public record PendingDecisionSummaryDto(Guid DecisionId, decimal Amount, string Source, DateTime CreatedAt);
public record ChildPendingDecisionsResult(Guid ChildId, List<PendingDecisionSummaryDto> Decisions);
