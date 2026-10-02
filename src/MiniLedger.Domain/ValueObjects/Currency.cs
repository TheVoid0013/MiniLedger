namespace MiniLedger.Domain.ValueObjects;

public sealed record Currency
{
    public string Code { get; }

    public Currency(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.");
        Code = code.ToUpperInvariant();
    }

    public static Currency NPR => new("NPR");
    public override string ToString() => Code;
}