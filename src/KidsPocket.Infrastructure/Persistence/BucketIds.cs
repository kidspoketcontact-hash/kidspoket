namespace KidsPocket.Infrastructure.Persistence;

// מזהים קבועים לשלוש קופות ברירת המחדל, כדי שקוד האפליקציה יוכל להתייחס אליהן
// בלי לחפש לפי מחרוזת בכל פעם. קופות מותאמות אישית לא צריכות מזהה קבוע.
public static class BucketIds
{
    public static readonly Guid Enjoy = new("00000000-0000-0000-0000-000000000001");
    public static readonly Guid Save = new("00000000-0000-0000-0000-000000000002");
    public static readonly Guid Grow = new("00000000-0000-0000-0000-000000000003");
}
