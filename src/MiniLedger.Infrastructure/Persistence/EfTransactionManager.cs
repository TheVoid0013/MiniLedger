namespace MiniLedger.Infrastructure.Persistence;

public sealed class EfTransactionManager(AppDbContext db) : ITransactionManager
{
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        if (db.Database.CurrentTransaction is not null)
            return await action();

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await action();
            await tx.CommitAsync(ct);
            return result;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}