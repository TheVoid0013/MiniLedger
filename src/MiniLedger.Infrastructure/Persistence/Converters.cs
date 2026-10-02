

namespace MiniLedger.Infrastructure.Persistence;

public sealed class CurrencyConverter()
    : ValueConverter<Currency, string>(c => c.Code, s => new Currency(s));

public sealed class AccountNumberConverter()
    : ValueConverter<AccountNumber, string>(a => a.Value, s => new AccountNumber(s));