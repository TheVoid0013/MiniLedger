namespace MiniLedger.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.AccountNumber).IsRequired();
        builder.HasIndex(a => a.AccountNumber).IsUnique();

        builder.Property(a => a.Owner).IsRequired().HasMaxLength(200);

        builder.Property(a => a.Version).IsConcurrencyToken();

        // Money -> two columns in the Accounts table
        builder.OwnsOne(a => a.Balance, money =>
        {
            money.Property(m => m.Amount).HasColumnName("Balance");
            money.Property(m => m.Currency).HasColumnName("Currency");
        });
        builder.Navigation(a => a.Balance).IsRequired();

        // Aggregate owns its transactions; EF writes to the private _transactions field
        builder.HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}