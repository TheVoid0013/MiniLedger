namespace MiniLedger.Application.Presentation;

public sealed record TransactionDto(Guid Id, string Type, decimal Amount, decimal BalanceAfter, Guid? Counterparty, DateTime CreatedAt);