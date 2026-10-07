using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace KidsPocket.Api.Tests;

public class AuthTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public AuthTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task ParentLogin_WithCorrectPassword_ReturnsTokenAndHouseholdId()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory);

        Assert.NotEqual(Guid.Empty, parent.HouseholdId);
        Assert.NotEqual(Guid.Empty, parent.AdultId);
        // אם ה-Client לא מכיל טוקן, קריאה מוגנת הייתה נכשלת - זו הוכחה עקיפה שההתחברות עבדה
        var response = await parent.Client.GetAsync($"/api/households/{parent.HouseholdId}/children");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ParentLogin_WithWrongPassword_Returns401()
    {
        var client = _factory.CreateClient();
        var email = $"{Guid.NewGuid()}@test.local";
        var household = await client.PostAsJsonAsync("/api/households", new { name = "בית לבדיקה" });
        var householdId = JsonDocument.Parse(await household.Content.ReadAsStringAsync()).RootElement.GetProperty("id").GetGuid();
        await client.PostAsJsonAsync($"/api/households/{householdId}/adults", new
        {
            displayName = "הורה", role = "Father", email, password = "CorrectPassword1!"
        });

        var response = await client.PostAsJsonAsync("/api/auth/parent/login", new { email, password = "WrongPassword!" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ParentLogin_WithUnknownEmail_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/parent/login", new { email = "nobody@nowhere.test", password = "whatever" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChildLogin_WithCorrectPin_ReturnsToken()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory);
        var childId = await AuthTestHelper.CreateChildAsync(parent, pin: "9876");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/child/login", new { childId, pin = "9876" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task ChildLogin_WithWrongPin_Returns401()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory);
        var childId = await AuthTestHelper.CreateChildAsync(parent, pin: "9876");

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/child/login", new { childId, pin = "0000" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChildLogin_WhenNoPinWasEverSet_Returns401()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory);
        var childId = await AuthTestHelper.CreateChildAsync(parent, pin: null);

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/child/login", new { childId, pin = "1234" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
