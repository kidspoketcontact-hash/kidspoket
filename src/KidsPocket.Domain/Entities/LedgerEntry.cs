using KidsPocket.Domain.Enums;

namespace KidsPocket.Domain.Entities;

// רשומת יומן - Append-only. לעולם לא נמחקת ולא מתעדכנת אחרי היצירה.
// ההיסטוריה המלאה היא חלק מה-Learning Journey של הילד.
public class LedgerEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid LedgerId { get; private set; }
    public Ledger? Ledger { get; private set; }

    public Guid BucketId { get; private set; }
    public Bucket? Bucket { get; private set; }

    // חיובי = כניסת כסף לקופה, שלילי = יציאת כסף (משיכה/הפקדה למטרה)
    public decimal Amount { get; private set; }
    public LedgerEntryType Type { get; private set; }

    public Guid? DecisionId { get; private set; }
    public Guid? SavingGoalId { get; private set; }
    public string? Description { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private LedgerEntry() { }

    public static LedgerEntry Create(
        Guid ledgerId, Guid bucketId, decimal amount, LedgerEntryType type,
        Guid? decisionId = null, Guid? savingGoalId = null, string? description = null)
    {
        if (amount == 0)
            throw new ArgumentException("סכום רשומת יומן לא יכול להיות אפס", nameof(amount));

        return new LedgerEntry
        {
            LedgerId = ledgerId,
            BucketId = bucketId,
            Amount = amount,
            Type = type,
            DecisionId = decisionId,
            SavingGoalId = savingGoalId,
            Description = description
        };
    }
}
