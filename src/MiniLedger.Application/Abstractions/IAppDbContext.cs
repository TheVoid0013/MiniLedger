namespace MiniLedger.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Account> Accounts { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
