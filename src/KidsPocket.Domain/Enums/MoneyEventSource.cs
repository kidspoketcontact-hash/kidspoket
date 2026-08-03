namespace KidsPocket.Domain.Enums;

// כל מקור שממנו יכול להגיע כסף לילד - כולם עוברים דרך אותו Decision Flow
public enum MoneyEventSource
{
    WeeklyAllowance = 1,
    MonthlyAllowance = 2,
    Chore = 3,
    BirthdayGift = 4,
    HolidayGift = 5,
    ManualReward = 6,
    Bonus = 7
}
