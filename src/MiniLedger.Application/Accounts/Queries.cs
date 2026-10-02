namespace MiniLedger.Application.Accounts;

public sealed record GetAccountQuery(Guid AccountId) : IQuery<AccountDto>;
public sealed record GetHistoryQuery(Guid AccountId) : IQuery<List<TransactionDto>>;

public sealed class GetAccountHandler(IAppDbContext db) : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery q, CancellationToken ct)
    {
        var a = await db.Accounts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == q.AccountId, ct)
                ?? throw new AccountNotFoundException(q.AccountId);
        return AccountDto.From(a);
    }
}

public sealed class GetHistoryHandler(IAppDbContext db) : IRequestHandler<GetHistoryQuery, List<TransactionDto>>
{
    public async Task<List<TransactionDto>> Handle(GetHistoryQuery q, CancellationToken ct)
    {
        var a = await db.Accounts.AsNoTracking().Include(x => x.Transactions)
                    .FirstOrDefaultAsync(x => x.Id == q.AccountId, ct)
                ?? throw new AccountNotFoundException(q.AccountId);

        return a.Transactions.OrderByDescending(t => t.CreatedAt)
            .Select(t => new TransactionDto(t.Id, t.Type.ToString(), t.Amount.Amount,
                t.BalanceAfter.Amount, t.CounterpartyAccountId, t.CreatedAt))
            .ToList();
    }
}