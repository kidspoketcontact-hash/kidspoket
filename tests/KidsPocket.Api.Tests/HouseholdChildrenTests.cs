using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace KidsPocket.Api.Tests;

public class HouseholdChildrenTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public HouseholdChildrenTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task GetChildren_AfterAddingTwoChildren_ListsBoth()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחת לוי");

        await parent.Client.PostAsJsonAsync($"/api/households/{parent.HouseholdId}/children", new { displayName = "נועה" });
        await parent.Client.PostAsJsonAsync($"/api/households/{parent.HouseholdId}/children", new { displayName = "איתן" });

        var response = await parent.Client.GetAsync($"/api/households/{parent.HouseholdId}/children");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var names = body.RootElement.GetProperty("children").EnumerateArray()
            .Select(c => c.GetProperty("displayName").GetString())
            .ToList();
        Assert.Equal(2, names.Count);
        Assert.Contains("נועה", names);
        Assert.Contains("איתן", names);
    }

    [Fact]
    public async Task GetChildren_Unauthenticated_Returns401()
    {
        var response = await _factory.CreateClient().GetAsync($"/api/households/{Guid.NewGuid()}/children");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetChildren_ForAnotherHousehold_Returns403()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory, "משפחת לוי");

        var response = await parent.Client.GetAsync($"/api/households/{Guid.NewGuid()}/children");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
