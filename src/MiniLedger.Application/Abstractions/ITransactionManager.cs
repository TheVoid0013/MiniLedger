namespace MiniLedger.Application.Abstractions;

public interface ITransactionManager
{
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct);
}