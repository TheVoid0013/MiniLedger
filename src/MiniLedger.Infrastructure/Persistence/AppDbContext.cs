namespace MiniLedger.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<Account> Accounts => Set<Account>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    // Value objects stored as plain columns, applied everywhere they appear (incl. owned types)
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<Currency>().HaveConversion<CurrencyConverter>().HaveMaxLength(3);
        builder.Properties<AccountNumber>().HaveConversion<AccountNumberConverter>().HaveMaxLength(12);
    }
}