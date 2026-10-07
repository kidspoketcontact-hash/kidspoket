using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace KidsPocket.Api.Tests;

public class ValidationTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;
    private readonly HttpClient _anonymousClient;

    public ValidationTests(ApiTestFactory factory)
    {
        _factory = factory;
        _anonymousClient = factory.CreateClient();
    }

    [Fact]
    public async Task CreateMoneyEvent_WithUnknownSource_Returns400WithErrorMessage()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory);
        var childId = await AuthTestHelper.CreateChildAsync(parent);

        var response = await parent.Client.PostAsJsonAsync("/api/money-events", new
        {
            childId,
            source = "NotARealSource",
            amount = 50m,
            description = (string?)null
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.False(string.IsNullOrWhiteSpace(body.RootElement.GetProperty("error").GetString()));
    }

    [Fact]
    public async Task Allocate_WhenSumDoesNotMatchMoneyEventAmount_Returns400()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory);
        var childId = await AuthTestHelper.CreateChildAsync(parent);
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

        var response = await childClient.PostAsJsonAsync($"/api/decisions/{decisionId}/allocate", new
        {
            allocations = new[] { new { bucketId = Guid.NewGuid(), amount = 10m } }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingDecision_Unauthenticated_Returns401()
    {
        var response = await _anonymousClient.GetAsync($"/api/decisions/{Guid.NewGuid()}/pending");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingDecision_ForUnknownDecisionId_Returns403ForAuthenticatedChildWithNoAccessToIt()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory);
        var childId = await AuthTestHelper.CreateChildAsync(parent);
        var childClient = await AuthTestHelper.LoginChildAsync(_factory, childId);

        var response = await childClient.GetAsync($"/api/decisions/{Guid.NewGuid()}/pending");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddAdult_WithUnknownRole_Returns400()
    {
        var household = await _anonymousClient.PostAsJsonAsync("/api/households", new { name = "בית לבדיקה" });
        var householdId = await GetIdAsync(household);

        var response = await _anonymousClient.PostAsJsonAsync($"/api/households/{householdId}/adults", new
        {
            displayName = "הורה",
            role = "Parent", // לא ערך תקין ב-AdultRole (Mother/Father/Stepparent/Grandparent/Guardian/Other)
            email = "parent@test.com",
            password = "Passw0rd!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static async Task<Guid> GetIdAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetGuid();
    }
}
