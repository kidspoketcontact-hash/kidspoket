using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;

namespace KidsPocket.Domain.Entities;

// כמה ימים אחרי החלטה - שואלים את הילד איך הוא מרגיש לגביה. חלק מ-Learning Journey.
public class DecisionReflection
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid DecisionId { get; private set; }
    public Decision? Decision { get; private set; }

    public DateTime ScheduledAt { get; private set; }
    public ReflectionSentiment? Sentiment { get; private set; }
    public DateTime? AnsweredAt { get; private set; }

    private DecisionReflection() { }

    public static DecisionReflection Schedule(Guid decisionId, DateTime scheduledAt) => new()
    {
        DecisionId = decisionId,
        ScheduledAt = scheduledAt
    };

    public void Answer(ReflectionSentiment sentiment)
    {
        if (AnsweredAt is not null)
            throw new DomainException("כבר ניתנה תשובה לרפלקציה הזו");

        Sentiment = sentiment;
        AnsweredAt = DateTime.UtcNow;
    }
}
