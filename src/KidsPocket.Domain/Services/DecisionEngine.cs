using KidsPocket.Domain.Entities;

namespace KidsPocket.Domain.Services;

// הליבה של הפילוסופיה החינוכית: כל כניסת כסף מתחילה בהצעת הקצאה ראשונית -
// 10% לכל קופת ברירת מחדל - והשאר פתוח לגמרי לילד להחליט.
// המנוע הזה לעולם לא אומר "טעות" - הוא רק מציע ומחשב.
public static class DecisionEngine
{
    public const decimal DefaultBucketSeedPercent = 0.10m;

    // בונה הצעת הקצאה ראשונית: 10% לכל קופת ברירת מחדל, השאר לא מוקצה
    public static IReadOnlyDictionary<Guid, decimal> BuildInitialSuggestion(
        decimal moneyEventAmount, IReadOnlyCollection<Bucket> defaultBuckets)
    {
        var suggestion = new Dictionary<Guid, decimal>();
        foreach (var bucket in defaultBuckets.Where(b => b.IsSystemDefault))
        {
            suggestion[bucket.Id] = Math.Round(moneyEventAmount * DefaultBucketSeedPercent, 2);
        }
        return suggestion;
    }

    // כמה כסף עדיין לא מוקצה מתוך ה-MoneyEvent, לפי הצעת הבסיס
    public static decimal CalculateUnallocatedRemainder(
        decimal moneyEventAmount, IReadOnlyDictionary<Guid, decimal> currentAllocation)
    {
        return moneyEventAmount - currentAllocation.Values.Sum();
    }

    // מסביר בשפה חינוכית מה ההשלכה של הזזת עוד סכום לקופת חיסכון על מטרה מסוימת
    public static string ExplainSavingImpact(decimal additionalAmountToSave, decimal weeklyContributionRate, decimal remainingToGoal)
    {
        if (weeklyContributionRate <= 0 || additionalAmountToSave <= 0)
            return "כל סכום שתעביר לחיסכון יקרב אותך למטרה.";

        var currentWeeksNeeded = Math.Ceiling(remainingToGoal / weeklyContributionRate);
        var newWeeksNeeded = Math.Ceiling(remainingToGoal / (weeklyContributionRate + additionalAmountToSave));
        var weeksSaved = currentWeeksNeeded - newWeeksNeeded;

        return weeksSaved > 0
            ? $"אם תעביר עוד {additionalAmountToSave:0.##} לחיסכון, תגיע למטרה כ-{weeksSaved:0} שבועות מוקדם יותר."
            : "הזזת הסכום הזה לא תשנה משמעותית מתי תגיע למטרה.";
    }
}
