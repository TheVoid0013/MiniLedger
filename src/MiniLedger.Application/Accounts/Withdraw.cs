namespace MiniLedger.Application.Accounts;


public sealed record WithdrawCommand(Guid AccountId, decimal Amount, string Currency) : ICommand<AccountDto>;

public sealed class WithdrawHandler(IAppDbContext db) : IRequestHandler<WithdrawCommand, AccountDto>
{
    public async Task<AccountDto> Handle(WithdrawCommand cmd, CancellationToken ct)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == cmd.AccountId, ct)
                      ?? throw new AccountNotFoundException(cmd.AccountId);

        account.Withdraw(new Money(cmd.Amount, new Currency(cmd.Currency)));
        await db.SaveChangesAsync(ct);
        return AccountDto.From(account);
    }
}