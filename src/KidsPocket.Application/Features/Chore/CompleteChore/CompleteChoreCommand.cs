namespace KidsPocket.Application.Features.Chore.CompleteChore;

// הילד מסמן שהמשימה בוצעה - עדיין לא מזכה בכסף עד שמבוגר יאשר
public record CompleteChoreCommand(Guid ChoreId);

public record CompleteChoreResult(Guid ChoreId, string Status);
