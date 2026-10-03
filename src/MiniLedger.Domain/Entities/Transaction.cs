namespace MiniLedger.Domain.Entities;

public enum TransactionType { Deposit, Withdrawal, TransferIn, TransferOut }

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Amount { get; private set; } = default!;
    public Money BalanceAfter { get; private set; } = default!;
    public Guid? CounterpartyAccountId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Transaction() { }

    internal static Transaction Create(Guid accountId, TransactionType type, Money amount,
        Money balanceAfter, Guid? counterparty = null) => new()
    {
        Id = Guid.NewGuid(),
        AccountId = accountId,
        Type = type,
        Amount = amount with { },              // fresh instance per row
        BalanceAfter = balanceAfter with { },  // fresh instance per row
        CounterpartyAccountId = counterparty,
        CreatedAt = DateTime.UtcNow
    };
}