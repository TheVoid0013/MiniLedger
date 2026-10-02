namespace MiniLedger.Application.Accounts;

public sealed record CreateAccountCommand(string Owner, string Currency) : ICommand<AccountDto>;

public sealed class CreateAccountHandler(IAppDbContext db) : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand cmd, CancellationToken ct)
    {
        var account = Account.Open(cmd.Owner, new Currency(cmd.Currency));
        db.Accounts.Add(account);
        await db.SaveChangesAsync(ct);
        return AccountDto.From(account);
    }
}