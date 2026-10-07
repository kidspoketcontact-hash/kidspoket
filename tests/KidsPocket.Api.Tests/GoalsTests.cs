using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace KidsPocket.Api.Tests;

public class GoalsTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public GoalsTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateContributeAndList_ReflectsProgressOnTheChildGoalsEndpoint()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחת גולדברג");
        var childId = await AuthTestHelper.CreateChildAsync(parent, "רוני");
        var childClient = await AuthTestHelper.LoginChildAsync(_factory, childId);

        var moneyEvent = await parent.Client.PostAsJsonAsync("/api/money-events", new
        {
            childId,
            source = "WeeklyAllowance",
            amount = 100m,
            description = (string?)null
        });
        var moneyEventBody = JsonDocument.Parse(await moneyEvent.Content.ReadAsStringAsync());
        var decisionId = moneyEventBody.RootElement.GetProperty("decisionId").GetGuid();
        var pending = JsonDocument.Parse(await (await childClient.GetAsync($"/api/decisions/{decisionId}/pending")).Content.ReadAsStringAsync());
        var saveId = pending.RootElement.GetProperty("initialSuggestion").EnumerateArray()
            .Single(s => s.GetProperty("bucketCode").GetString() == "save").GetProperty("bucketId").GetGuid();
        await childClient.PostAsJsonAsync($"/api/decisions/{decisionId}/allocate", new
        {
            allocations = new[] { new { bucketId = saveId, amount = 100m } }
        });

        var createGoal = await childClient.PostAsJsonAsync("/api/goals", new { childId, name = "אופניים", targetAmount = 50m, targetDate = (DateTime?)null });
        Assert.Equal(HttpStatusCode.OK, createGoal.StatusCode);
        var goalId = JsonDocument.Parse(await createGoal.Content.ReadAsStringAsync()).RootElement.GetProperty("goalId").GetGuid();

        var contribute = await childClient.PostAsJsonAsync($"/api/goals/{goalId}/contribute", new { amount = 30m });
        Assert.Equal(HttpStatusCode.OK, contribute.StatusCode);

        // ההורה גם יכול לצפות (לא רק הילד) - זה חלק מהרעיון של לוח הבקרה להורים
        var listResponse = await parent.Client.GetAsync($"/api/children/{childId}/goals");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var goals = JsonDocument.Parse(await listResponse.Content.ReadAsStringAsync()).RootElement.GetProperty("goals").EnumerateArray().ToList();
        var goal = Assert.Single(goals);
        Assert.Equal(30m, goal.GetProperty("currentAmount").GetDecimal());
        Assert.False(goal.GetProperty("isCompleted").GetBoolean());
    }

    [Fact]
    public async Task GetChildGoals_ForChildNotInYourHousehold_Returns403()
    {
        var parentA = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחה א");
        var parentB = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחה ב");
        var childOfA = await AuthTestHelper.CreateChildAsync(parentA, "ילד של א");

        var response = await parentB.Client.GetAsync($"/api/children/{childOfA}/goals");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
