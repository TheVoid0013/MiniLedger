using MiniLedger.Domain.Exceptions;

namespace MiniLedger.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative.");
        Amount = decimal.Round(amount, 2);
        Currency = currency;
    }

    public static Money Zero(Currency c) => new(0, c);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency); // negative throws
    }

    public bool IsGreaterThan(Money other) { EnsureSameCurrency(other); return Amount > other.Amount; }
    public bool IsZero => Amount == 0;

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new CurrencyMismatchException(Currency, other.Currency);
    }
}