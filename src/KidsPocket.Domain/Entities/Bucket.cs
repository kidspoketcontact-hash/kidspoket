namespace KidsPocket.Domain.Entities;

// קופה - Enjoy / Save / Grow הן ברירת המחדל, אבל המבנה תומך בקופות מותאמות אישית
// (Donation, Education, Travel...) בלי שינוי סכימה, כי זו טבלה ולא Enum קשיח.
public class Bucket
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty; // "enjoy" | "save" | "grow" | custom
    public string DisplayName { get; private set; } = string.Empty;
    public bool IsSystemDefault { get; private set; }
    public int SortOrder { get; private set; }

    private Bucket() { }

    public static Bucket CreateSystemDefault(string code, string displayName, int sortOrder) => new()
    {
        Code = code,
        DisplayName = displayName,
        IsSystemDefault = true,
        SortOrder = sortOrder
    };

    public static Bucket CreateCustom(string code, string displayName, int sortOrder) => new()
    {
        Code = code,
        DisplayName = displayName,
        IsSystemDefault = false,
        SortOrder = sortOrder
    };
}
