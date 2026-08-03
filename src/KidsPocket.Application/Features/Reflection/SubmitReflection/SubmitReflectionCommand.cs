namespace KidsPocket.Application.Features.Reflection.SubmitReflection;

public record SubmitReflectionCommand(Guid DecisionId, string Sentiment);

public record SubmitReflectionResult(Guid DecisionId, string Sentiment, DateTime AnsweredAt);
