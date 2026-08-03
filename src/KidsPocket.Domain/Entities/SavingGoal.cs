namespace KidsPocket.Domain.Entities;

// המטרה עצמה. CurrentAmount לא מאוחסן - נגזר תמיד מסכום LedgerEntries מסוג GoalContribution
// ששייכות ל-Id הזה. השדה כאן הוא cache אופציונלי בלבד לנוחות שאילתות, לא מקור האמת.
public class SavingGoal
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ChildId { get; private set; }
    public Child? Child { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public decimal TargetAmount { get; private set; }
    public DateTime? TargetDate { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private SavingGoal() { }

    public static SavingGoal Create(Guid childId, string name, decimal targetAmount, DateTime? targetDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("שם המטרה לא יכול להיות ריק", nameof(name));
        if (targetAmount <= 0)
            throw new ArgumentException("סכום היעד חייב להיות חיובי", nameof(targetAmount));

        return new SavingGoal
        {
            ChildId = childId,
            Name = name,
            TargetAmount = targetAmount,
            TargetDate = targetDate
        };
    }

    // נקרא אחרי חישוב הסכום המצטבר בפועל מה-Ledger, כדי לעדכן את הדגל
    public void RefreshCompletionStatus(decimal currentAmountFromLedger)
    {
        IsCompleted = currentAmountFromLedger >= TargetAmount;
    }
}
