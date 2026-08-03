namespace KidsPocket.Domain.Exceptions;

// חריגה לכל הפרה של כלל עסקי בשכבת ה-Domain
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
