namespace MiniLedger.Domain.Tests.Entities;

public class TransactionTests
{
    private static Money NprMoney(decimal amount) => new(amount, Currency.NPR);

    [Fact]
    public void Create_MapsAllFields()
    {
        var accountId = Guid.NewGuid();
        var amount = NprMoney(40);
        var balanceAfter = NprMoney(160);

        var tx = Transaction.Create(accountId, TransactionType.Deposit, amount, balanceAfter);

        Assert.Equal(accountId, tx.AccountId);
        Assert.Equal(TransactionType.Deposit, tx.Type);
        Assert.Equal(amount, tx.Amount);
        Assert.Equal(balanceAfter, tx.BalanceAfter);
    }

    [Fact]
    public void Create_WithoutCounterparty_LeavesItNull()
    {
        var tx = Transaction.Create(Guid.NewGuid(), TransactionType.Deposit, NprMoney(10), NprMoney(10));

        Assert.Null(tx.CounterpartyAccountId);
    }

    [Fact]
    public void Create_WithCounterparty_StoresIt()
    {
        var counterparty = Guid.NewGuid();

        var tx = Transaction.Create(
            Guid.NewGuid(), TransactionType.TransferOut, NprMoney(10), NprMoney(0), counterparty);

        Assert.Equal(counterparty, tx.CounterpartyAccountId);
    }

    [Theory]
    [InlineData(TransactionType.Deposit)]
    [InlineData(TransactionType.Withdrawal)]
    [InlineData(TransactionType.TransferIn)]
    [InlineData(TransactionType.TransferOut)]
    public void Create_StoresEachTransactionType(TransactionType type)
    {
        var tx = Transaction.Create(Guid.NewGuid(), type, NprMoney(10), NprMoney(10));

        Assert.Equal(type, tx.Type);
    }

    [Fact]
    public void Create_GeneratesUniqueNonEmptyIds()
    {
        var accountId = Guid.NewGuid();

        var first = Transaction.Create(accountId, TransactionType.Deposit, NprMoney(10), NprMoney(10));
        var second = Transaction.Create(accountId, TransactionType.Deposit, NprMoney(10), NprMoney(20));

        Assert.NotEqual(Guid.Empty, first.Id);
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Create_SetsCreatedAtToCurrentUtcTime()
    {
        var before = DateTime.UtcNow;

        var tx = Transaction.Create(Guid.NewGuid(), TransactionType.Deposit, NprMoney(10), NprMoney(10));

        var after = DateTime.UtcNow;
        Assert.Equal(DateTimeKind.Utc, tx.CreatedAt.Kind);
        Assert.InRange(tx.CreatedAt, before, after);
    }

    [Fact]
    public void Create_StoresCopiesOfAmountAndBalanceAfter()
    {
        var amount = NprMoney(40);
        var balanceAfter = NprMoney(160);

        var tx = Transaction.Create(Guid.NewGuid(), TransactionType.Deposit, amount, balanceAfter);

        Assert.NotSame(amount, tx.Amount);
        Assert.NotSame(balanceAfter, tx.BalanceAfter);
        Assert.Equal(amount, tx.Amount);               // same value, different instance
        Assert.Equal(balanceAfter, tx.BalanceAfter);
    }
}