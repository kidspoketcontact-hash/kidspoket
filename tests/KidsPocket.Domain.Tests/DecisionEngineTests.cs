using KidsPocket.Domain.Entities;
using KidsPocket.Domain.Services;
using Xunit;

namespace KidsPocket.Domain.Tests;

public class DecisionEngineTests
{
    private static readonly Bucket Enjoy = Bucket.CreateSystemDefault("enjoy", "Enjoy", 1);
    private static readonly Bucket Save = Bucket.CreateSystemDefault("save", "Save", 2);
    private static readonly Bucket Grow = Bucket.CreateSystemDefault("grow", "Grow", 3);

    [Fact]
    public void BuildInitialSuggestion_GivesTenPercentToEachDefaultBucket()
    {
        var suggestion = DecisionEngine.BuildInitialSuggestion(100m, new[] { Enjoy, Save, Grow });

        Assert.Equal(10m, suggestion[Enjoy.Id]);
        Assert.Equal(10m, suggestion[Save.Id]);
        Assert.Equal(10m, suggestion[Grow.Id]);
    }

    [Fact]
    public void CalculateUnallocatedRemainder_ReturnsRestAfterDefaultSuggestion()
    {
        var suggestion = DecisionEngine.BuildInitialSuggestion(100m, new[] { Enjoy, Save, Grow });
        var remainder = DecisionEngine.CalculateUnallocatedRemainder(100m, suggestion);

        Assert.Equal(70m, remainder);
    }
}
