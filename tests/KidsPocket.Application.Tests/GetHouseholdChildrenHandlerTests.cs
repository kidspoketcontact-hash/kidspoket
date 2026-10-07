using KidsPocket.Application.Features.Household.GetHouseholdChildren;
using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Exceptions;
using Xunit;

namespace KidsPocket.Application.Tests;

public class GetHouseholdChildrenHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllChildrenLinkedToTheHousehold()
    {
        var db = TestDb.Create();
        var household = Household.Create("משפחת כהן");
        db.Households.Add(household);
        await db.SaveChangesAsync();

        var first = await TestSeed.CreateChildWithLedgerAsync(db, "דני");
        db.ChildAdults.Add(ChildAdult.Create(first.Id, household.Id));
        var second = await TestSeed.CreateChildWithLedgerAsync(db, "מיה");
        db.ChildAdults.Add(ChildAdult.Create(second.Id, household.Id));
        await db.SaveChangesAsync();

        var handler = new GetHouseholdChildrenHandler(db);
        var result = await handler.HandleAsync(new GetHouseholdChildrenQuery(household.Id));

        Assert.Equal(2, result.Children.Count);
        Assert.Contains(result.Children, c => c.DisplayName == "דני");
        Assert.Contains(result.Children, c => c.DisplayName == "מיה");
    }

    [Fact]
    public async Task Handle_DoesNotReturnChildrenFromOtherHouseholds()
    {
        var db = TestDb.Create();
        var household = Household.Create("משפחת כהן");
        var otherHousehold = Household.Create("משפחה אחרת");
        db.Households.AddRange(household, otherHousehold);
        await db.SaveChangesAsync();

        var ownChild = await TestSeed.CreateChildWithLedgerAsync(db, "דני");
        db.ChildAdults.Add(ChildAdult.Create(ownChild.Id, household.Id));
        var otherChild = await TestSeed.CreateChildWithLedgerAsync(db, "ילד אחר");
        db.ChildAdults.Add(ChildAdult.Create(otherChild.Id, otherHousehold.Id));
        await db.SaveChangesAsync();

        var result = await new GetHouseholdChildrenHandler(db).HandleAsync(new GetHouseholdChildrenQuery(household.Id));

        Assert.Single(result.Children);
        Assert.Equal("דני", result.Children[0].DisplayName);
    }

    [Fact]
    public async Task Handle_WhenHouseholdDoesNotExist_ThrowsDomainException()
    {
        var db = TestDb.Create();
        var handler = new GetHouseholdChildrenHandler(db);

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new GetHouseholdChildrenQuery(Guid.NewGuid())));
    }
}
