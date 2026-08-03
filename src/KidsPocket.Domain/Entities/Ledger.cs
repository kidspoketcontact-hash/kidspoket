namespace KidsPocket.Domain.Entities;

// יומן - נכס אחד לכל ילד. היתרה לעולם לא מאוחסנת כאן, רק נגזרת מ-LedgerEntries.
public class Ledger
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ChildId { get; private set; }
    public Child? Child { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public ICollection<LedgerEntry> Entries { get; private set; } = new List<LedgerEntry>();

    private Ledger() { }

    public static Ledger CreateForChild(Guid childId) => new() { ChildId = childId };
}
