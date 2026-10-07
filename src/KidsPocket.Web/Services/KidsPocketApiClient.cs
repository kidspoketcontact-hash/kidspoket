using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KidsPocket.Contracts.Allowance;
using KidsPocket.Contracts.Decision;
using KidsPocket.Contracts.Ledger;
using KidsPocket.Web.Models;

namespace KidsPocket.Web.Services;

public class KidsPocketApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly ClientAuthState _authState;

    public KidsPocketApiClient(IHttpClientFactory factory, ClientAuthState authState)
    {
        _http = factory.CreateClient("KidsPocketApi");
        _authState = authState;
    }

    // --- הרשמה / התחברות - אלה תמיד אנונימיים, אין עדיין טוקן לצרף ---

    public Task<CreateHouseholdResult> CreateHouseholdAsync(string name) =>
        PostAsync<CreateHouseholdApiRequest, CreateHouseholdResult>("api/households", new CreateHouseholdApiRequest(name));

    public Task<AddAdultResultDto> AddAdultAsync(Guid householdId, string displayName, string role, string email, string password) =>
        PostAsync<AddAdultApiRequest, AddAdultResultDto>(
            $"api/households/{householdId}/adults", new AddAdultApiRequest(displayName, role, email, password));

    public Task<ParentLoginResultDto> ParentLoginAsync(string email, string password) =>
        PostAsync<ParentLoginApiRequest, ParentLoginResultDto>("api/auth/parent/login", new ParentLoginApiRequest(email, password));

    public Task<ChildLoginResultDto> ChildLoginAsync(Guid childId, string pin) =>
        PostAsync<ChildLoginApiRequest, ChildLoginResultDto>("api/auth/child/login", new ChildLoginApiRequest(childId, pin));

    public Task<ExternalLoginResultDto> GoogleLoginAsync(string idToken, string? householdName) =>
        PostAsync<GoogleLoginApiRequest, ExternalLoginResultDto>("api/auth/google", new GoogleLoginApiRequest(idToken, householdName));

    public Task<ExternalLoginResultDto> AppleLoginAsync(string idToken, string? householdName, string? email, string? displayName) =>
        PostAsync<AppleLoginApiRequest, ExternalLoginResultDto>("api/auth/apple", new AppleLoginApiRequest(idToken, householdName, email, displayName));

    // --- שאר ה-API - דורש טוקן, מצורף אוטומטית מ-ClientAuthState ---

    public Task<CreateChildResult> CreateChildAsync(Guid householdId, string displayName, string? pin) =>
        PostAsync<CreateChildApiRequest, CreateChildResult>(
            $"api/households/{householdId}/children", new CreateChildApiRequest(displayName, pin));

    public Task<HouseholdChildrenResult> GetHouseholdChildrenAsync(Guid householdId) =>
        GetAsync<HouseholdChildrenResult>($"api/households/{householdId}/children");

    public Task<ChildBalanceResponse> GetBalanceAsync(Guid childId) =>
        GetAsync<ChildBalanceResponse>($"api/children/{childId}/balance");

    public Task<CreateMoneyEventResult> CreateMoneyEventAsync(Guid childId, string source, decimal amount, string? description) =>
        PostAsync<CreateMoneyEventRequest, CreateMoneyEventResult>(
            "api/money-events", new CreateMoneyEventRequest(childId, source, amount, description));

    public Task<ChildPendingDecisionsResult> GetChildPendingDecisionsAsync(Guid childId) =>
        GetAsync<ChildPendingDecisionsResult>($"api/children/{childId}/decisions/pending");

    public Task<PendingDecisionResponse> GetPendingDecisionAsync(Guid decisionId) =>
        GetAsync<PendingDecisionResponse>($"api/decisions/{decisionId}/pending");

    public Task<AllocateMoneyResultDto> AllocateAsync(Guid decisionId, IReadOnlyList<BucketAllocationInput> allocations) =>
        PostAsync<AllocateMoneyRequest, AllocateMoneyResultDto>(
            $"api/decisions/{decisionId}/allocate", new AllocateMoneyRequest(allocations));

    public Task<ChildGoalsResult> GetChildGoalsAsync(Guid childId) =>
        GetAsync<ChildGoalsResult>($"api/children/{childId}/goals");

    public Task<CreateGoalResultDto> CreateGoalAsync(Guid childId, string name, decimal targetAmount, DateTime? targetDate) =>
        PostAsync<CreateGoalApiRequest, CreateGoalResultDto>(
            "api/goals", new CreateGoalApiRequest(childId, name, targetAmount, targetDate));

    public Task<ContributeToGoalResultDto> ContributeToGoalAsync(Guid goalId, decimal amount) =>
        PostAsync<ContributeToGoalApiRequest, ContributeToGoalResultDto>(
            $"api/goals/{goalId}/contribute", new ContributeToGoalApiRequest(amount));

    public Task<ChildChoresResult> GetChildChoresAsync(Guid childId) =>
        GetAsync<ChildChoresResult>($"api/children/{childId}/chores");

    public Task<CreateChoreResultDto> CreateChoreAsync(Guid childId, Guid createdByAdultId, string title, string? description, decimal rewardAmount) =>
        PostAsync<CreateChoreApiRequest, CreateChoreResultDto>(
            "api/chores", new CreateChoreApiRequest(childId, createdByAdultId, title, description, rewardAmount));

    public Task<CompleteChoreResultDto> CompleteChoreAsync(Guid choreId) =>
        PostAsync<object, CompleteChoreResultDto>($"api/chores/{choreId}/complete", new { });

    public Task<ApproveChoreResultDto> ApproveChoreAsync(Guid choreId) =>
        PostAsync<object, ApproveChoreResultDto>($"api/chores/{choreId}/approve", new { });

    public Task<CreateRewardResultDto> CreateRewardAsync(Guid childId, Guid createdByAdultId, decimal amount, string? description) =>
        PostAsync<CreateRewardApiRequest, CreateRewardResultDto>(
            "api/rewards", new CreateRewardApiRequest(childId, createdByAdultId, amount, description));

    private async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest body)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };
        ApplyAuthHeader(request);
        var response = await _http.SendAsync(request);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions))!;
    }

    private async Task<TResponse> GetAsync<TResponse>(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyAuthHeader(request);
        var response = await _http.SendAsync(request);
        await EnsureSuccessAsync(response);
        return (await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions))!;
    }

    // בונים HttpRequestMessage ידנית (במקום PostAsJsonAsync/GetAsync) כדי שכל קריאה תישא
    // את הטוקן העדכני מ-ClientAuthState - הוא יכול להשתנות (login/logout) לאורך חיי ה-circuit.
    private void ApplyAuthHeader(HttpRequestMessage request)
    {
        if (_authState.Token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authState.Token);
    }

    // ה-API ממיר DomainException ל-400 עם { "error": "..." } - חושפים את ההודעה הזו למשתמש במקום גוף JSON גולמי
    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();
        try
        {
            var problem = JsonSerializer.Deserialize<Dictionary<string, string>>(body, JsonOptions);
            if (problem is not null && problem.TryGetValue("error", out var error))
                throw new InvalidOperationException(error);
        }
        catch (JsonException) { /* falls through to raw body below */ }

        var message = response.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "צריך להתחבר מחדש.",
            System.Net.HttpStatusCode.Forbidden => "אין הרשאה לפעולה הזו.",
            _ => $"{(int)response.StatusCode}: {body}"
        };
        throw new InvalidOperationException(message);
    }
}
