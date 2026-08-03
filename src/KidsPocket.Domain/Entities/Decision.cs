using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;

namespace KidsPocket.Domain.Entities;

// כל MoneyEvent יוצר בדיוק Decision אחת. הילד מקצה את הכסף בין הקופות ומאשר.
public class Decision
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid MoneyEventId { get; private set; }
    public MoneyEvent? MoneyEvent { get; private set; }
    public Guid ChildId { get; private set; }

    public DecisionStatus Status { get; private set; } = DecisionStatus.Pending;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; private set; }

    public ICollection<DecisionAllocation> Allocations { get; private set; } = new List<DecisionAllocation>();
    public DecisionReflection? Reflection { get; private set; }

    private Decision() { }

    public static Decision CreateForMoneyEvent(Guid moneyEventId, Guid childId) => new()
    {
        MoneyEventId = moneyEventId,
        ChildId = childId
    };

    // מאשר את ההחלטה עם ההקצאה הסופית שהילד בחר. הסכום חייב להיות שווה במדויק לסכום ה-MoneyEvent
    // (זו הבדיקה היחידה - אין "הקצאה נכונה" או "לא נכונה", רק שהכסף כולו שובץ).
    public void Confirm(IReadOnlyDictionary<Guid, decimal> finalAllocationByBucketId, decimal moneyEventAmount)
    {
        if (Status == DecisionStatus.Confirmed)
            throw new DomainException("ההחלטה כבר אושרה");

        var total = finalAllocationByBucketId.Values.Sum();
        if (total != moneyEventAmount)
            throw new DomainException("סכום ההקצאה חייב להיות שווה בדיוק לסכום שהתקבל");

        if (finalAllocationByBucketId.Values.Any(v => v < 0))
            throw new DomainException("הקצאה לקופה לא יכולה להיות שלילית");

        foreach (var (bucketId, amount) in finalAllocationByBucketId)
        {
            if (amount == 0) continue;
            Allocations.Add(DecisionAllocation.Create(Id, bucketId, amount));
        }

        Status = DecisionStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void ScheduleReflection(DateTime scheduledAt)
    {
        if (Status != DecisionStatus.Confirmed)
            throw new DomainException("אפשר לתזמן רפלקציה רק אחרי אישור ההחלטה");

        Reflection = DecisionReflection.Schedule(Id, scheduledAt);
    }
}
