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
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Adult() { }

    public static Adult Create(Guid householdId, string displayName, AdultRole role, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("שם המבוגר לא יכול להיות ריק", nameof(displayName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("אימייל לא יכול להיות ריק", nameof(email));

        return new Adult
        {
            HouseholdId = householdId,
            DisplayName = displayName,
            Role = role,
            Email = email,
            PasswordHash = passwordHash
        };
    }
}
