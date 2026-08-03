using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Exceptions;
using Xunit;

namespace KidsPocket.Domain.Tests;

public class DecisionTests
{
    [Fact]
    public void Confirm_ThrowsWhenAllocationDoesNotMatchMoneyEventAmount()
    {
        var decision = Decision.CreateForMoneyEvent(Guid.NewGuid(), Guid.NewGuid());
        var allocations = new Dictionary<Guid, decimal> { [Guid.NewGuid()] = 50m };

        Assert.Throws<DomainException>(() => decision.Confirm(allocations, 100m));
    }

    [Fact]
    public void Confirm_SucceedsWhenAllocationMatchesExactly()
    {
        var decision = Decision.CreateForMoneyEvent(Guid.NewGuid(), Guid.NewGuid());
        var bucketId = Guid.NewGuid();
        var allocations = new Dictionary<Guid, decimal> { [bucketId] = 100m };

        decision.Confirm(allocations, 100m);

        Assert.Single(decision.Allocations);
        Assert.Equal(Enums.DecisionStatus.Confirmed, decision.Status);
    }
}
