namespace MiniLedger.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public sealed class InsufficientFundsException()
    : DomainException("Insufficient funds.");

public sealed class CurrencyMismatchException(Currency a, Currency b)
    : DomainException($"Currency mismatch: {a} vs {b}.");

public sealed class InvalidAmountException()
    : DomainException("Amount must be greater than zero.");

public sealed class AccountNotFoundException(Guid id)
    : DomainException($"Account {id} not found.");

public sealed class SameAccountTransferException()
    : DomainException("Cannot transfer to the same account.");
    