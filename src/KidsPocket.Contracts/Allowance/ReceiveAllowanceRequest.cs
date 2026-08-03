namespace KidsPocket.Contracts.Allowance;

// אדמין/מבוגר יוצר אירוע כספי חדש לילד - חד"פ (תגמול, מתנה) או ידני מחוץ ל-AllowancePlan
public record CreateMoneyEventRequest(Guid ChildId, string Source, decimal Amount, string? Description);

public record MoneyEventResponse(Guid Id, Guid ChildId, string Source, decimal Amount, string? Description, Guid DecisionId, DateTime CreatedAt);
