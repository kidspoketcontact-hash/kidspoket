using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace KidsPocket.Api.Tests;

public record ParentSession(HttpClient Client, Guid HouseholdId, Guid AdultId);

// כל בקרים בפרויקט (חוץ מ-AuthController וחלקי OnboardingController שהם ה"הרשמה") דורשים
// עכשיו טוקן - הטסטים צריכים להירשם/להתחבר לפני שהם יכולים לקרוא לשאר ה-API.
public static class AuthTestHelper
{
    public const string DefaultPin = "1234";

    public static async Task<ParentSession> RegisterAndLoginParentAsync(ApiTestFactory factory, string householdName = "משפחת בדיקה")
    {
        var setupClient = factory.CreateClient();
        var email = $"{Guid.NewGuid()}@test.local";
        const string password = "Passw0rd!";

        var household = await setupClient.PostAsJsonAsync("/api/households", new { name = householdName });
        var householdId = await GetGuidAsync(household, "id");

        await setupClient.PostAsJsonAsync($"/api/households/{householdId}/adults", new
        {
            displayName = "הורה בדיקה",
            role = "Mother",
            email,
            password
        });

        var login = await setupClient.PostAsJsonAsync("/api/auth/parent/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var body = JsonDocument.Parse(await login.Content.ReadAsStringAsync()).RootElement;

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.GetProperty("token").GetString());

        return new ParentSession(client, householdId, body.GetProperty("adultId").GetGuid());
    }

    public static async Task<Guid> CreateChildAsync(ParentSession parent, string displayName = "דני", string? pin = DefaultPin)
    {
        var response = await parent.Client.PostAsJsonAsync(
            $"/api/households/{parent.HouseholdId}/children", new { displayName, pin });
        return await GetGuidAsync(response, "id");
    }

    public static async Task<HttpClient> LoginChildAsync(ApiTestFactory factory, Guid childId, string pin = DefaultPin)
    {
        var setupClient = factory.CreateClient();
        var login = await setupClient.PostAsJsonAsync("/api/auth/child/login", new { childId, pin });
        login.EnsureSuccessStatusCode();
        var token = JsonDocument.Parse(await login.Content.ReadAsStringAsync()).RootElement.GetProperty("token").GetString();

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public static async Task<Guid> GetGuidAsync(HttpResponseMessage response, string property)
    {
        response.EnsureSuccessStatusCode();
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty(property).GetGuid();
    }
}
