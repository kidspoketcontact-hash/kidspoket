namespace KidsPocket.Application.Features.Allowance.ReceiveAllowance;

// כל כסף נכנס - דמי כיס, מתנה, בונוס - יוצר MoneyEvent + Decision. אף פעם לא נכנס ישירות לארנק.
public record ReceiveAllowanceCommand(Guid ChildId, string Source, decimal Amount, string? Description);

public record ReceiveAllowanceResult(Guid MoneyEventId, Guid DecisionId, decimal Amount, DateTime CreatedAt);
