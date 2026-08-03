namespace KidsPocket.Application.Abstractions;

// CQRS קליל בלי תלות חיצונית (בלי MediatR) - כל Slice מגדיר Command/Query + Handler משלו
public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken ct = default);
}

public interface IQueryHandler<in TQuery, TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken ct = default);
}
