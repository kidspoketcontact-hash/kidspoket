namespace KidsPocket.Domain.Entities;

public class Child
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string DisplayName { get; private set; } = string.Empty;
    public string? PinHash { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // לילד יש ארנק (Ledger) אחד יחיד, גם אם הוא שייך למספר בתי אב
    public Ledger? Ledger { get; private set; }

    public ICollection<ChildAdult> AdultMemberships { get; private set; } = new List<ChildAdult>();
    public ICollection<SavingGoal> SavingGoals { get; private set; } = new List<SavingGoal>();
    public ICollection<Chore> Chores { get; private set; } = new List<Chore>();

    private Child() { }

    public static Child Create(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("שם הילד לא יכול להיות ריק", nameof(displayName));

        return new Child { DisplayName = displayName };
    }

    public void SetPin(string pinHash) => PinHash = pinHash;
}
