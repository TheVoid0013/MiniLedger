using MiniLedger.Domain.Entities;
using MiniLedger.Domain.Exceptions;
using MiniLedger.Domain.ValueObjects;

namespace MiniLedger.Domain.Tests.Entities;

public class AccountTests
{
    private static Money NprMoney(decimal amount) => new(amount, Currency.NPR);

    private static Account OpenWithBalance(decimal balance)
    {
        var account = Account.Open("Alice", Currency.NPR);
        if (balance > 0) account.Deposit(NprMoney(balance));
        return account;
    }

    // ---------- Open ----------

    [Fact]
    public void Open_WithValidOwner_CreatesEmptyAccount()
    {
        var account = Account.Open("Alice", Currency.NPR);

        Assert.NotEqual(Guid.Empty, account.Id);
        Assert.Equal(12, account.AccountNumber.Value.Length);
        Assert.Equal("Alice", account.Owner);
        Assert.Equal(Money.Zero(Currency.NPR), account.Balance);
        Assert.Equal(0, account.Version);
        Assert.Empty(account.Transactions);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Open_WithBlankOwner_Throws(string? owner)
    {
        Assert.Throws<ArgumentException>(() => Account.Open(owner!, Currency.NPR));
    }

    // ---------- Deposit ----------

    [Fact]
    public void Deposit_ValidAmount_IncreasesBalanceAndRecordsTransaction()
    {
        var account = Account.Open("Alice", Currency.NPR);

        account.Deposit(NprMoney(100));

        Assert.Equal(NprMoney(100), account.Balance);
        Assert.Equal(1, account.Version);
        var tx = Assert.Single(account.Transactions);
        Assert.Equal(TransactionType.Deposit, tx.Type);
        Assert.Equal(account.Id, tx.AccountId);
        Assert.Equal(NprMoney(100), tx.Amount);
        Assert.Equal(NprMoney(100), tx.BalanceAfter);
        Assert.Null(tx.CounterpartyAccountId);
    }

    [Fact]
    public void Deposit_Twice_SumsBalanceAndKeepsOrder()
    {
        var account = Account.Open("Alice", Currency.NPR);

        account.Deposit(NprMoney(100));
        account.Deposit(NprMoney(50));

        Assert.Equal(NprMoney(150), account.Balance);
        Assert.Equal(2, account.Version);
        var txs = account.Transactions.ToList();
        Assert.Equal(NprMoney(100), txs[0].BalanceAfter);
        Assert.Equal(NprMoney(150), txs[1].BalanceAfter);
    }

    [Fact]
    public void Deposit_ZeroAmount_ThrowsAndChangesNothing()
    {
        var account = OpenWithBalance(100);

        Assert.Throws<InvalidAmountException>(() => account.Deposit(NprMoney(0)));

        Assert.Equal(NprMoney(100), account.Balance);
        Assert.Equal(1, account.Version);
        Assert.Single(account.Transactions);
    }

    [Fact]
    public void Deposit_DifferentCurrency_Throws()
    {
        var account = OpenWithBalance(100);
        var usd = new Money(10, new Currency("USD"));

        Assert.Throws<CurrencyMismatchException>(() => account.Deposit(usd));

        Assert.Equal(NprMoney(100), account.Balance);
        Assert.Equal(1, account.Version);
    }

    // ---------- Withdraw ----------

    [Fact]
    public void Withdraw_LessThanBalance_DecreasesBalanceAndRecordsTransaction()
    {
        var account = OpenWithBalance(100);

        account.Withdraw(NprMoney(30));

        Assert.Equal(NprMoney(70), account.Balance);
        Assert.Equal(2, account.Version);
        var tx = account.Transactions.Last();
        Assert.Equal(TransactionType.Withdrawal, tx.Type);
        Assert.Equal(NprMoney(30), tx.Amount);
        Assert.Equal(NprMoney(70), tx.BalanceAfter);
    }

    [Fact]
    public void Withdraw_EqualToBalance_LeavesZero()
    {
        var account = OpenWithBalance(100);

        account.Withdraw(NprMoney(100));

        Assert.True(account.Balance.IsZero);
    }

    [Fact]
    public void Withdraw_MoreThanBalance_ThrowsAndChangesNothing()
    {
        var account = OpenWithBalance(100);

        Assert.Throws<InsufficientFundsException>(() => account.Withdraw(NprMoney(100.01m)));

        Assert.Equal(NprMoney(100), account.Balance);
        Assert.Equal(1, account.Version);
        Assert.Single(account.Transactions);
    }

    [Fact]
    public void Withdraw_ZeroAmount_Throws()
    {
        var account = OpenWithBalance(100);

        Assert.Throws<InvalidAmountException>(() => account.Withdraw(NprMoney(0)));
    }

    [Fact]
    public void Withdraw_DifferentCurrency_Throws()
    {
        var account = OpenWithBalance(100);
        var usd = new Money(10, new Currency("USD"));

        Assert.Throws<CurrencyMismatchException>(() => account.Withdraw(usd));
    }

    // ---------- Transfer methods (internal) ----------

    [Fact]
    public void DebitForTransfer_RecordsTransferOutWithCounterparty()
    {
        var account = OpenWithBalance(100);
        var counterparty = Guid.NewGuid();

        account.DebitForTransfer(NprMoney(40), counterparty);

        Assert.Equal(NprMoney(60), account.Balance);
        var tx = account.Transactions.Last();
        Assert.Equal(TransactionType.TransferOut, tx.Type);
        Assert.Equal(counterparty, tx.CounterpartyAccountId);
    }

    [Fact]
    public void CreditFromTransfer_RecordsTransferInWithCounterparty()
    {
        var account = Account.Open("Bob", Currency.NPR);
        var counterparty = Guid.NewGuid();

        account.CreditFromTransfer(NprMoney(40), counterparty);

        Assert.Equal(NprMoney(40), account.Balance);
        var tx = Assert.Single(account.Transactions);
        Assert.Equal(TransactionType.TransferIn, tx.Type);
        Assert.Equal(counterparty, tx.CounterpartyAccountId);
    }

    [Fact]
    public void DebitForTransfer_InsufficientFunds_ThrowsAndChangesNothing()
    {
        var account = OpenWithBalance(100);

        Assert.Throws<InsufficientFundsException>(
            () => account.DebitForTransfer(NprMoney(200), Guid.NewGuid()));

        Assert.Equal(NprMoney(100), account.Balance);
        Assert.Single(account.Transactions);
    }

    [Fact]
    public void TransferMethods_ZeroAmount_Throw()
    {
        var account = OpenWithBalance(100);

        Assert.Throws<InvalidAmountException>(() => account.DebitForTransfer(NprMoney(0), Guid.NewGuid()));
        Assert.Throws<InvalidAmountException>(() => account.CreditFromTransfer(NprMoney(0), Guid.NewGuid()));
    }

    // ---------- Encapsulation ----------

    [Fact]
    public void Transactions_IsReadOnlyView()
    {
        var account = OpenWithBalance(100);

        var collection = (ICollection<Transaction>)account.Transactions;

        Assert.True(collection.IsReadOnly);
    }
}