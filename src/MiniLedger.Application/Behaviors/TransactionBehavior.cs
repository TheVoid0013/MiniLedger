namespace MiniLedger.Application.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(ITransactionManager tx)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
        => tx.ExecuteInTransactionAsync(() => next(), ct);
}