namespace MiniLedger.Domain.Services;

public class TransferService
{
    public void Transfer(Account source, Account destination, Money amount)
    {
        if (source.Id == destination.Id)
            throw new SameAccountTransferException();

        source.DebitForTransfer(amount, destination.Id);
        destination.CreditFromTransfer(amount, source.Id);
    }
}