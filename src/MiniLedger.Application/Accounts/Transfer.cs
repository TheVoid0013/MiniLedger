namespace MiniLedger.Application.Accounts;

public sealed record TransferCommand(Guid SourceId, Guid DestinationId, decimal Amount, string Currency)
    : ICommand<Unit>;

public sealed class TransferHandler(IAppDbContext db, TransferService transferService)
    : IRequestHandler<TransferCommand, Unit>
{
    public async Task<Unit> Handle(TransferCommand cmd, CancellationToken ct)
    {
        var ids = new[] { cmd.SourceId, cmd.DestinationId };
        var accounts = await db.Accounts.Where(a => ids.Contains(a.Id)).ToListAsync(ct);

        var source = accounts.FirstOrDefault(a => a.Id == cmd.SourceId)
                     ?? throw new AccountNotFoundException(cmd.SourceId);
        var destination = accounts.FirstOrDefault(a => a.Id == cmd.DestinationId)
                          ?? throw new AccountNotFoundException(cmd.DestinationId);

        transferService.Transfer(source, destination, new Money(cmd.Amount, new Currency(cmd.Currency)));
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}