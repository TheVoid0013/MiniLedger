namespace MiniLedger.Infrastructure.Persistence.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();   // important, see note below

        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.CreatedAt).IsRequired();

        builder.OwnsOne(t => t.Amount, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Amount");
            money.Property(m => m.Currency).HasColumnName("AmountCurrency");
        });
        builder.Navigation(t => t.Amount).IsRequired();

        builder.OwnsOne(t => t.BalanceAfter, money =>
        {
            money.Property(m => m.Amount).HasColumnName("BalanceAfter");
            money.Property(m => m.Currency).HasColumnName("BalanceAfterCurrency");
        });
        builder.Navigation(t => t.BalanceAfter).IsRequired();

        builder.HasIndex(t => new { t.AccountId, t.CreatedAt });
    }
}