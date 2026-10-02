namespace MiniLedger.Application.Presentation;

public sealed record AccountDto(Guid Id, string AccountNumber, string Owner, decimal Balance, string Currency)
{
    public static AccountDto From(Account a) =>
        new(a.Id, a.AccountNumber.Value, a.Owner, a.Balance.Amount, a.Balance.Currency.Code);
}