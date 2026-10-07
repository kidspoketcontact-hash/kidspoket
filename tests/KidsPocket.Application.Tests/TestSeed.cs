using KidsPocket.Domain.Entities;
using KidsPocket.Infrastructure.Persistence;

namespace KidsPocket.Application.Tests;

public static class TestSeed
{
    // אותו רצף כמו ב-OnboardingController.AddChild - ילד תמיד נולד עם ארנק משלו
    public static async Task<Child> CreateChildWithLedgerAsync(KidsPocketDbContext db, string displayName = "דני")
    {
        var child = Child.Create(displayName);
        db.Children.Add(child);

        var ledger = Ledger.CreateForChild(child.Id);
        db.Ledgers.Add(ledger);

        await db.SaveChangesAsync();
        return child;
    }
}
