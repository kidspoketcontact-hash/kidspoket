namespace KidsPocket.Web.Services;

// מצב ה-Auth של המשתמש הנוכחי, בהיקף Scoped (מופע אחד לכל SignalR circuit ב-Blazor Server -
// בדיוק הגרנולריות הנכונה ל"סשן משתמש נוכחי", בלי צורך ב-localStorage/cookies).
public class ClientAuthState
{
    public string? Token { get; private set; }
    public string? Role { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? HouseholdId { get; private set; }
    public string? DisplayName { get; private set; }

    public bool IsAuthenticated => Token is not null;
    public bool IsParent => Role == "Parent";
    public bool IsChild => Role == "Child";

    public event Action? Changed;

    public void SetParent(string token, Guid adultId, Guid householdId, string displayName)
    {
        Token = token;
        Role = "Parent";
        UserId = adultId;
        HouseholdId = householdId;
        DisplayName = displayName;
        Changed?.Invoke();
    }

    public void SetChild(string token, Guid childId, string displayName)
    {
        Token = token;
        Role = "Child";
        UserId = childId;
        HouseholdId = null;
        DisplayName = displayName;
        Changed?.Invoke();
    }

    public void Clear()
    {
        Token = null;
        Role = null;
        UserId = null;
        HouseholdId = null;
        DisplayName = null;
        Changed?.Invoke();
    }
}
