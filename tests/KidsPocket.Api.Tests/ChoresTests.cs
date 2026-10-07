using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace KidsPocket.Api.Tests;

public class ChoresTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public ChoresTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task FullLifecycle_CreateCompleteApprove_EndsWithPendingDecisionAndListedAsApproved()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחת אבני");
        var childId = await AuthTestHelper.CreateChildAsync(parent, "עומר");
        var childClient = await AuthTestHelper.LoginChildAsync(_factory, childId);

        var createChore = await parent.Client.PostAsJsonAsync("/api/chores", new
        {
            childId,
            createdByAdultId = parent.AdultId,
            title = "לשטוף כלים",
            description = (string?)null,
            rewardAmount = 12m
        });
        Assert.Equal(HttpStatusCode.OK, createChore.StatusCode);
        var choreId = JsonDocument.Parse(await createChore.Content.ReadAsStringAsync()).RootElement.GetProperty("choreId").GetGuid();

        var listBeforeComplete = await parent.Client.GetAsync($"/api/children/{childId}/chores");
        var beforeChores = JsonDocument.Parse(await listBeforeComplete.Content.ReadAsStringAsync()).RootElement.GetProperty("chores").EnumerateArray().ToList();
        Assert.Equal("Pending", Assert.Single(beforeChores).GetProperty("status").GetString());

        // הילד מסמן שביצע
        var complete = await childClient.PostAsync($"/api/chores/{choreId}/complete", null);
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);

        // ורק ההורה מאשר ומזכה
        var approve = await parent.Client.PostAsync($"/api/chores/{choreId}/approve", null);
        Assert.Equal(HttpStatusCode.OK, approve.StatusCode);
        var approveBody = JsonDocument.Parse(await approve.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(12m, approveBody.GetProperty("amount").GetDecimal());
        var decisionId = approveBody.GetProperty("decisionId").GetGuid();

        var pending = await childClient.GetAsync($"/api/decisions/{decisionId}/pending");
        Assert.Equal(HttpStatusCode.OK, pending.StatusCode);

        var listAfter = await parent.Client.GetAsync($"/api/children/{childId}/chores");
        var afterChores = JsonDocument.Parse(await listAfter.Content.ReadAsStringAsync()).RootElement.GetProperty("chores").EnumerateArray().ToList();
        Assert.Equal("Approved", Assert.Single(afterChores).GetProperty("status").GetString());
    }

    [Fact]
    public async Task Approve_BeforeCompleted_Returns400()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחת אבני");
        var childId = await AuthTestHelper.CreateChildAsync(parent, "עומר");
        var createChore = await parent.Client.PostAsJsonAsync("/api/chores", new
        {
            childId,
            createdByAdultId = parent.AdultId,
            title = "לקפל כביסה",
            description = (string?)null,
            rewardAmount = 8m
        });
        var choreId = JsonDocument.Parse(await createChore.Content.ReadAsStringAsync()).RootElement.GetProperty("choreId").GetGuid();

        var approve = await parent.Client.PostAsync($"/api/chores/{choreId}/approve", null);

        Assert.Equal(HttpStatusCode.BadRequest, approve.StatusCode);
    }

    [Fact]
    public async Task Complete_AsParent_Returns403SinceOnlyTheChildCanMarkItDone()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחת אבני");
        var childId = await AuthTestHelper.CreateChildAsync(parent, "עומר");
        var createChore = await parent.Client.PostAsJsonAsync("/api/chores", new
        {
            childId,
            createdByAdultId = parent.AdultId,
            title = "לסדר ספרים",
            description = (string?)null,
            rewardAmount = 5m
        });
        var choreId = JsonDocument.Parse(await createChore.Content.ReadAsStringAsync()).RootElement.GetProperty("choreId").GetGuid();

        var complete = await parent.Client.PostAsync($"/api/chores/{choreId}/complete", null);

        Assert.Equal(HttpStatusCode.Forbidden, complete.StatusCode);
    }
}
