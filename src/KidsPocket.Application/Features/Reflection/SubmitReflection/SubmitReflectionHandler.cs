using KidsPocket.Application.Abstractions;
using KidsPocket.Domain.Enums;
using KidsPocket.Domain.Exceptions;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Application.Features.Reflection.SubmitReflection;

public class SubmitReflectionHandler : ICommandHandler<SubmitReflectionCommand, SubmitReflectionResult>
{
    private readonly KidsPocketDbContext _db;

    public SubmitReflectionHandler(KidsPocketDbContext db) => _db = db;

    public async Task<SubmitReflectionResult> HandleAsync(SubmitReflectionCommand command, CancellationToken ct = default)
    {
        if (!Enum.TryParse<ReflectionSentiment>(command.Sentiment, ignoreCase: true, out var sentiment))
            throw new DomainException($"תחושה לא מוכרת: {command.Sentiment}");

        var decision = await _db.Decisions
            .Include(d => d.Reflection)
            .SingleOrDefaultAsync(d => d.Id == command.DecisionId, ct)
            ?? throw new DomainException("החלטה לא נמצאה");

        if (decision.Reflection is null)
            throw new DomainException("לא תוזמנה רפלקציה להחלטה הזו");

        decision.Reflection.Answer(sentiment);
        await _db.SaveChangesAsync(ct);

        return new SubmitReflectionResult(decision.Id, sentiment.ToString(), decision.Reflection.AnsweredAt!.Value);
    }
}
