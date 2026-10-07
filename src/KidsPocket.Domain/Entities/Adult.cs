using KidsPocket.Domain.Enums;

namespace KidsPocket.Domain.Entities;

public class Adult
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid HouseholdId { get; private set; }
    public Household? Household { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;
    public AdultRole Role { get; private set; }
    public string Email { get; private set; } = string.Empty;

    // ריק עבור Adult שנרשם רק דרך Google/Apple (אין סיסמה מקומית שלו)
    public string? PasswordHash { get; private set; }

    // Local = יש PasswordHash, אין ExternalId. Google/Apple = יש ExternalId (ה-sub מה-ID token
    // שלהם), PasswordHash יכול להיות null. אדם יכול "לקשר" ספק חיצוני לחשבון מקומי קיים -
    // ה-Provider/ExternalId פשוט מתעדכנים על אותו Adult, לא נוצרת רשומה כפולה.
    public AuthProvider Provider { get; private set; } = AuthProvider.Local;
    public string? ExternalId { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Adult() { }

    public static Adult Create(Guid householdId, string displayName, AdultRole role, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("שם המבוגר לא יכול להיות ריק", nameof(displayName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("אימייל לא יכול להיות ריק", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("סיסמה לא יכולה להיות ריקה", nameof(passwordHash));

        return new Adult
        {
            HouseholdId = householdId,
            DisplayName = displayName,
            Role = role,
            Email = email,
            PasswordHash = passwordHash,
            Provider = AuthProvider.Local
        };
    }

    public static Adult CreateExternal(Guid householdId, string displayName, AdultRole role, string email, AuthProvider provider, string externalId)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("שם המבוגר לא יכול להיות ריק", nameof(displayName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("אימייל לא יכול להיות ריק", nameof(email));
        if (provider == AuthProvider.Local)
            throw new ArgumentException("ספק חיצוני לא יכול להיות Local", nameof(provider));
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("מזהה חיצוני לא יכול להיות ריק", nameof(externalId));

        return new Adult
        {
            HouseholdId = householdId,
            DisplayName = displayName,
            Role = role,
            Email = email,
            Provider = provider,
            ExternalId = externalId
        };
    }

    // מקשר ספק חיצוני (Google/Apple) לחשבון קיים - למשל הורה שנרשם באימייל/סיסמה ואז מתחבר
    // פעם ראשונה עם Google באותו אימייל. לא נוגע ב-PasswordHash - הכניסה המקומית ממשיכה לעבוד.
    public void LinkExternalProvider(AuthProvider provider, string externalId)
    {
        if (provider == AuthProvider.Local)
            throw new ArgumentException("ספק חיצוני לא יכול להיות Local", nameof(provider));
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("מזהה חיצוני לא יכול להיות ריק", nameof(externalId));

        Provider = provider;
        ExternalId = externalId;
    }
}
