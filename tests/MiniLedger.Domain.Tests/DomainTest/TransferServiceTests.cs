namespace MiniLedger.Domain.Tests.Services;

public class TransferServiceTests
{
    private readonly TransferService _service = new();

    private static Money NprMoney(decimal amount) => new(amount, Currency.NPR);

    private static Account OpenWithBalance(string owner, decimal balance)
    {
        var account = Account.Open(owner, Currency.NPR);
        if (balance > 0) account.Deposit(NprMoney(balance));
        return account;
    }
    
    [Fact]
    public void Transfer_MovesMoneyBetweenAccounts()
    {
        var source = OpenWithBalance("Alice", 100);
        var destination = OpenWithBalance("Bob", 10);

        _service.Transfer(source, destination, NprMoney(40));

        Assert.Equal(NprMoney(60), source.Balance);
        Assert.Equal(NprMoney(50), destination.Balance);
    }

    [Fact]
    public void Transfer_RecordsTransferOutAndTransferInWithCounterparties()
    {
        var source = OpenWithBalance("Alice", 100);
        var destination = OpenWithBalance("Bob", 0);

        _service.Transfer(source, destination, NprMoney(40));

        var out_ = source.Transactions.Last();
        Assert.Equal(TransactionType.TransferOut, out_.Type);
        Assert.Equal(destination.Id, out_.CounterpartyAccountId);

        var in_ = destination.Transactions.Last();
        Assert.Equal(TransactionType.TransferIn, in_.Type);
        Assert.Equal(source.Id, in_.CounterpartyAccountId);
    }

    [Fact]
    public void Transfer_ToSameAccount_Throws()
    {
        var account = OpenWithBalance("Alice", 100);

        Assert.Throws<SameAccountTransferException>(
            () => _service.Transfer(account, account, NprMoney(10)));

        Assert.Equal(NprMoney(100), account.Balance);
    }

    [Fact]
    public void Transfer_InsufficientFunds_ThrowsAndChangesNeitherAccount()
    {
        var source = OpenWithBalance("Alice", 100);
        var destination = OpenWithBalance("Bob", 10);

        Assert.Throws<InsufficientFundsException>(
            () => _service.Transfer(source, destination, NprMoney(500)));

        Assert.Equal(NprMoney(100), source.Balance);
        Assert.Equal(NprMoney(10), destination.Balance);
    }

    // EXPECTED TO FAIL with the current TransferService (see note below).
    [Fact]
    public void Transfer_DestinationInDifferentCurrency_ThrowsAndLeavesSourceUntouched()
    {
        var source = OpenWithBalance("Alice", 100);
        var destination = Account.Open("Bob", new Currency("USD"));

        Assert.Throws<CurrencyMismatchException>(
            () => _service.Transfer(source, destination, NprMoney(50)));

        Assert.Equal(NprMoney(100), source.Balance);
        Assert.Empty(destination.Transactions);
    }
}