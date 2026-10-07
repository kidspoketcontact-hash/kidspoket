using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace KidsPocket.Api.Tests;

// אותו מסלול שנבדק ידנית עם curl בתחילת הפרויקט - עכשיו כטסט קבוע, כדי שרגרסיה
// עתידית ב-endpoint כלשהו לאורך המסלול תיתפס אוטומטית. עכשיו גם עובר דרך login אמיתי:
// ההורה נותן את הכסף, הילד הוא זה שמחליט ומאשר את ההקצאה.
public class DecisionFlowHappyPathTests : IClassFixture<ApiTestFactory>
{
    private readonly ApiTestFactory _factory;

    public DecisionFlowHappyPathTests(ApiTestFactory factory) => _factory = factory;

    [Fact]
    public async Task FullHappyPath_HouseholdToAllocationToBalanceToReflection_Succeeds()
    {
        var parent = await AuthTestHelper.RegisterAndLoginParentAsync(_factory);
        var childId = await AuthTestHelper.CreateChildAsync(parent, "דני");
        var childClient = await AuthTestHelper.LoginChildAsync(_factory, childId);

        var moneyEvent = await PostAsync(parent.Client, "/api/money-events", new
        {
            childId,
            source = "WeeklyAllowance",
            amount = 100m,
            description = "דמי כיס"
        });
        var decisionId = moneyEvent.RootElement.GetProperty("decisionId").GetGuid();

        // ההורה הוא זה שיצר את הכסף - הילד צריך לגלות שיש לו החלטה ממתינה כשהוא נכנס
        var childPending = await GetAsync(childClient, $"/api/children/{childId}/decisions/pending");
        var pendingSummary = Assert.Single(childPending.RootElement.GetProperty("decisions").EnumerateArray());
        Assert.Equal(decisionId, pendingSummary.GetProperty("decisionId").GetGuid());

        var pending = await GetAsync(childClient, $"/api/decisions/{decisionId}/pending");
        Assert.Equal(100m, pending.RootElement.GetProperty("totalAmount").GetDecimal());
        Assert.Equal(70m, pending.RootElement.GetProperty("unallocatedRemainder").GetDecimal());
        var suggestions = pending.RootElement.GetProperty("initialSuggestion").EnumerateArray().ToList();
        Assert.Equal(3, suggestions.Count);
        var enjoyId = suggestions.Single(s => s.GetProperty("bucketCode").GetString() == "enjoy").GetProperty("bucketId").GetGuid();
        var saveId = suggestions.Single(s => s.GetProperty("bucketCode").GetString() == "save").GetProperty("bucketId").GetGuid();
        var growId = suggestions.Single(s => s.GetProperty("bucketCode").GetString() == "grow").GetProperty("bucketId").GetGuid();

        var allocateResponse = await childClient.PostAsJsonAsync($"/api/decisions/{decisionId}/allocate", new
        {
            allocations = new[]
            {
                new { bucketId = enjoyId, amount = 10m },
                new { bucketId = saveId, amount = 80m },
                new { bucketId = growId, amount = 10m },
            }
        });
        Assert.Equal(HttpStatusCode.OK, allocateResponse.StatusCode);
        var allocate = JsonDocument.Parse(await allocateResponse.Content.ReadAsStringAsync());
        Assert.Equal("Confirmed", allocate.RootElement.GetProperty("status").GetString());

        var balance = await GetAsync(childClient, $"/api/children/{childId}/balance");
        Assert.Equal(100m, balance.RootElement.GetProperty("totalBalance").GetDecimal());

        var reflectionResponse = await childClient.PostAsJsonAsync($"/api/decisions/{decisionId}/reflection", new { sentiment = "Happy" });
        Assert.Equal(HttpStatusCode.OK, reflectionResponse.StatusCode);
    }

    private static async Task<JsonDocument> PostAsync(HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    private static async Task<JsonDocument> GetAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
