namespace KidsPocket.Domain.Entities;

// תיעוד ה-snapshot הסופי של הקצאה לקופה בזמן אישור ה-Decision
public class DecisionAllocation
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DecisionId { get; private set; }
    public Decision? Decision { get; private set; }
    public Guid BucketId { get; private set; }
    public Bucket? Bucket { get; private set; }
    public decimal Amount { get; private set; }

    private DecisionAllocation() { }

    public static DecisionAllocation Create(Guid decisionId, Guid bucketId, decimal amount) => new()
    {
        DecisionId = decisionId,
        BucketId = bucketId,
        Amount = amount
    };
}
