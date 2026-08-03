using KidsPocket.Domain.Entities;

namespace KidsPocket.Domain.Services;

// היתרה בכל קופה תמיד מחושבת מחדש מרשומות היומן - היא לעולם לא מאוחסנת.
public static class LedgerCalculator
{
    public static decimal CalculateBucketBalance(IEnumerable<LedgerEntry> entries, Guid bucketId) =>
        entries.Where(e => e.BucketId == bucketId).Sum(e => e.Amount);

    public static IReadOnlyDictionary<Guid, decimal> CalculateAllBucketBalances(IEnumerable<LedgerEntry> entries) =>
        entries.GroupBy(e => e.BucketId).ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

    public static decimal CalculateTotalBalance(IEnumerable<LedgerEntry> entries) =>
        entries.Sum(e => e.Amount);

    // הפקדה למטרה נשמרת כערך שלילי מול קופת Save (כסף שיוחד למטרה, יצא מה"מאגר הפתוח"),
    // לכן ההתקדמות בפועל היא הערך המוחלט של סכום כל ההפקדות שתויגו למטרה הזו.
    public static decimal CalculateGoalProgress(IEnumerable<LedgerEntry> entries, Guid savingGoalId) =>
        -entries.Where(e => e.SavingGoalId == savingGoalId).Sum(e => e.Amount);
}
