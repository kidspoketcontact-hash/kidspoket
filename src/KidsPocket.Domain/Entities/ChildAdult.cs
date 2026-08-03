namespace KidsPocket.Domain.Entities;

// קישור בין ילד לבית אב - מאפשר לילד להשתייך למספר בתי אב (הורים גרושים וכו')
public class ChildAdult
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ChildId { get; private set; }
    public Child? Child { get; private set; }
    public Guid HouseholdId { get; private set; }
    public Household? Household { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private ChildAdult() { }

    public static ChildAdult Create(Guid childId, Guid householdId) =>
        new() { ChildId = childId, HouseholdId = householdId };
}
