using KidsPocket.Domain.Enums;

namespace KidsPocket.Domain.Entities;

// ההגדרה שמבוגר קובע: כמה, באיזו תדירות. לא קובעת איך הכסף מתחלק - זה תמיד ב-Decision.
public class AllowancePlan
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ChildId { get; private set; }
    public Child? Child { get; private set; }
    public Guid CreatedByAdultId { get; private set; }

    public decimal Amount { get; private set; }
    public AllowanceFrequency Frequency { get; private set; }
    public int ScheduleDay { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? LastTriggeredAt { get; private set; }

    private AllowancePlan() { }

    public static AllowancePlan Create(
        Guid childId, Guid createdByAdultId, decimal amount, AllowanceFrequency frequency, int scheduleDay)
    {
        if (amount <= 0)
            throw new ArgumentException("סכום דמי הכיס חייב להיות חיובי", nameof(amount));

        return new AllowancePlan
        {
            ChildId = childId,
            CreatedByAdultId = createdByAdultId,
            Amount = amount,
            Frequency = frequency,
            ScheduleDay = scheduleDay
        };
    }

    public void MarkTriggered() => LastTriggeredAt = DateTime.UtcNow;
    public void Deactivate() => IsActive = false;
}
