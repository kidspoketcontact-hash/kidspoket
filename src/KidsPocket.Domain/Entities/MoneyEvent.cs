using KidsPocket.Domain.Enums;

namespace KidsPocket.Domain.Entities;

// כל כניסת כסף לילד - מכל מקור שהוא - מתחילה כ-MoneyEvent.
// היא לעולם לא נכנסת ישירות לארנק; היא פותחת Decision.
public class MoneyEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ChildId { get; private set; }
    public Child? Child { get; private set; }

    public MoneyEventSource Source { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }

    public Guid? CreatedByAdultId { get; private set; }
    public Guid? ChoreId { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private MoneyEvent() { }

    public static MoneyEvent Create(
        Guid childId, MoneyEventSource source, decimal amount,
        Guid? createdByAdultId = null, Guid? choreId = null, string? description = null)
    {
        if (amount <= 0)
            throw new ArgumentException("סכום אירוע כספי חייב להיות חיובי", nameof(amount));

        return new MoneyEvent
        {
            ChildId = childId,
            Source = source,
            Amount = amount,
            CreatedByAdultId = createdByAdultId,
            ChoreId = choreId,
            Description = description
        };
    }
}
