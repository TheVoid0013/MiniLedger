namespace MiniLedger.Application.Accounts;

public sealed record DepositCommand(Guid AccountId, decimal Amount, string Currency) : ICommand<AccountDto>;

public sealed class DepositHandler(IAppDbContext db) : IRequestHandler<DepositCommand, AccountDto>
{
    public async Task<AccountDto> Handle(DepositCommand cmd, CancellationToken ct)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == cmd.AccountId, ct)
                      ?? throw new AccountNotFoundException(cmd.AccountId);

        account.Deposit(new Money(cmd.Amount, new Currency(cmd.Currency)));
        await db.SaveChangesAsync(ct);
        return AccountDto.From(account);
    }
}