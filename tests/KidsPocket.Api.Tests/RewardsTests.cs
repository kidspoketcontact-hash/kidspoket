using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace KidsPocket.Api.Tests;

public class RewardsTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public RewardsTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateReward_CreatesLinkedMoneyEventAndPendingDecision()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחת שני");
        var childId = await AuthTestHelper.CreateChildAsync(parent, "טל");
        var childClient = await AuthTestHelper.LoginChildAsync(_factory, childId);

        var reward = await parent.Client.PostAsJsonAsync("/api/rewards", new
        {
            childId,
            createdByAdultId = parent.AdultId,
            amount = 25m,
            description = "עזרה בבית"
        });
        Assert.Equal(HttpStatusCode.OK, reward.StatusCode);
        var body = JsonDocument.Parse(await reward.Content.ReadAsStringAsync()).RootElement;
        var decisionId = body.GetProperty("decisionId").GetGuid();

        var pending = await childClient.GetAsync($"/api/decisions/{decisionId}/pending");
        Assert.Equal(HttpStatusCode.OK, pending.StatusCode);
        var pendingBody = JsonDocument.Parse(await pending.Content.ReadAsStringAsync()).RootElement;
        Assert.Equal(25m, pendingBody.GetProperty("totalAmount").GetDecimal());
    }

    [Fact]
    public async Task CreateReward_AsChild_Returns403()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחת שני");
        var childId = await AuthTestHelper.CreateChildAsync(parent, "טל");
        var childClient = await AuthTestHelper.LoginChildAsync(_factory, childId);

        var reward = await childClient.PostAsJsonAsync("/api/rewards", new
        {
            childId,
            createdByAdultId = parent.AdultId,
            amount = 100m,
            description = "תגמול עצמי, כמובן שלא"
        });

        Assert.Equal(HttpStatusCode.Forbidden, reward.StatusCode);
    }

    private static async Task<Guid> GetIdAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetGuid();
    }
}
