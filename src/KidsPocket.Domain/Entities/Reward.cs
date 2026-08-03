namespace KidsPocket.Domain.Entities;

// תגמול ידני חד-פעמי שמבוגר נותן - גם הוא יוצר MoneyEvent ועובר דרך אותו Decision Flow
public class Reward
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ChildId { get; private set; }
    public Child? Child { get; private set; }
    public Guid CreatedByAdultId { get; private set; }

    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public Guid? MoneyEventId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Reward() { }

    public static Reward Create(Guid childId, Guid createdByAdultId, decimal amount, string? description)
    {
        if (amount <= 0)
            throw new ArgumentException("סכום התגמול חייב להיות חיובי", nameof(amount));

        return new Reward
        {
            ChildId = childId,
            CreatedByAdultId = createdByAdultId,
            Amount = amount,
            Description = description
        };
    }

    public void LinkMoneyEvent(Guid moneyEventId) => MoneyEventId = moneyEventId;
}
