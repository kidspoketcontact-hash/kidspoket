namespace KidsPocket.Domain.Entities;

// בית אב - יכולים להיות כמה עבור אותו ילד (הורים גרושים, משפחות מורכבות וכו')
public class Household
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<Adult> Adults { get; private set; } = new List<Adult>();
    public ICollection<ChildAdult> ChildMemberships { get; private set; } = new List<ChildAdult>();

    private Household() { }

    public static Household Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("שם בית האב לא יכול להיות ריק", nameof(name));

        return new Household { Name = name };
    }
}
