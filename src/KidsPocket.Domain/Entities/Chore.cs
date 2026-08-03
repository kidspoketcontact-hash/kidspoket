using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;

namespace KidsPocket.Domain.Entities;

public class Chore
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ChildId { get; private set; }
    public Child? Child { get; private set; }
    public Guid CreatedByAdultId { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal RewardAmount { get; private set; }
    public ChoreStatus Status { get; private set; } = ChoreStatus.Pending;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    private Chore() { }

    public static Chore Create(Guid childId, Guid createdByAdultId, string title, decimal rewardAmount, string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("שם המשימה לא יכול להיות ריק", nameof(title));
        if (rewardAmount <= 0)
            throw new ArgumentException("סכום התגמול חייב להיות חיובי", nameof(rewardAmount));

        return new Chore
        {
            ChildId = childId,
            CreatedByAdultId = createdByAdultId,
            Title = title,
            RewardAmount = rewardAmount,
            Description = description
        };
    }

    // הילד מסמן שביצע
    public void MarkCompleted()
    {
        if (Status != ChoreStatus.Pending)
            throw new DomainException("אפשר לסמן כבוצע רק משימה שממתינה");

        Status = ChoreStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    // המבוגר מאשר -> זו הנקודה שבה נוצר MoneyEvent (בשכבת ה-Application)
    public void Approve()
    {
        if (Status != ChoreStatus.Completed)
            throw new DomainException("אפשר לאשר רק משימה שסומנה כבוצעה");

        Status = ChoreStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
    }
}
