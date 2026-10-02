namespace MiniLedger.Api.Contracts;

public sealed record CreateAccountRequest(string Owner, string Currency);
public sealed record MoneyRequest(decimal Amount, string Currency);
public sealed record TransferRequest(Guid SourceAccountId, Guid DestinationAccountId, decimal Amount, string Currency);