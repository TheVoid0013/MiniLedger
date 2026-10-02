namespace MiniLedger.Domain.Entities;

public class Account
{
    private readonly List<Transaction> _transactions = new();

    public Guid Id { get; private set; }
    public AccountNumber AccountNumber { get; private set; } = default!;
    public string Owner { get; private set; } = default!;
    public Money Balance { get; private set; } = default!;
    public long Version { get; private set; }   // concurrency token
    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Account() { } // EF

    public static Account Open(string owner, Currency currency)
    {
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("Owner is required.");

        return new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = AccountNumber.Generate(),
            Owner = owner,
            Balance = Money.Zero(currency),
            Version = 0
        };
    }

    public void Deposit(Money money)
    {
        EnsurePositive(money);
        Balance = Balance.Add(money);
        _transactions.Add(Transaction.Create(Id, TransactionType.Deposit, money, Balance));
        Version++;
    }

    public void Withdraw(Money money)
    {
        EnsurePositive(money);
        if (money.IsGreaterThan(Balance)) throw new InsufficientFundsException();
        Balance = Balance.Subtract(money);
        _transactions.Add(Transaction.Create(Id, TransactionType.Withdrawal, money, Balance));
        Version++;
    }

    // used only by TransferService so the ledger shows "transfer", not deposit/withdraw
    internal void DebitForTransfer(Money money, Guid counterpartyId)
    {
        EnsurePositive(money);
        if (money.IsGreaterThan(Balance)) throw new InsufficientFundsException();
        Balance = Balance.Subtract(money);
        _transactions.Add(Transaction.Create(Id, TransactionType.TransferOut, money, Balance, counterpartyId));
        Version++;
    }

    internal void CreditFromTransfer(Money money, Guid counterpartyId)
    {
        EnsurePositive(money);
        Balance = Balance.Add(money);
        _transactions.Add(Transaction.Create(Id, TransactionType.TransferIn, money, Balance, counterpartyId));
        Version++;
    }

    private static void EnsurePositive(Money m)
    {
        if (m.IsZero) throw new InvalidAmountException();
    }
}