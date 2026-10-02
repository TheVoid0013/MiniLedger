namespace MiniLedger.Domain.ValueObjects;

public sealed record AccountNumber
{
    public string Value { get; }

    public AccountNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 12 || !value.All(char.IsDigit))
            throw new ArgumentException("Account number must be 12 digits.");
        Value = value;
    }

    public static AccountNumber Generate()
        => new(Random.Shared.NextInt64(100_000_000_000, 999_999_999_999).ToString());

    public override string ToString() => Value;
}