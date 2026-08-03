namespace KidsPocket.Application.Features.Chore.ApproveChore;

// המבוגר מאשר משימה שהילד סימן כבוצעה - זו הנקודה שיוצרת MoneyEvent + Decision
public record ApproveChoreCommand(Guid ChoreId);

public record ApproveChoreResult(Guid ChoreId, Guid MoneyEventId, Guid DecisionId, decimal Amount);
