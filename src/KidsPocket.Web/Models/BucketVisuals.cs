namespace KidsPocket.Web.Models;

// מיפוי קבוע Code -> אייקון/צבע, כדי לא לכפול את זה בכל מסך שמציג קופות
public static class BucketVisuals
{
    public static string IconFor(string bucketCode) => bucketCode switch
    {
        "enjoy" => "🎉",
        "save" => "🐷",
        "grow" => "🌱",
        _ => "💰"
    };

    public static string CssClassFor(string bucketCode) => bucketCode switch
    {
        "enjoy" => "bucket-enjoy",
        "save" => "bucket-save",
        "grow" => "bucket-grow",
        _ => "bucket-default"
    };
}
