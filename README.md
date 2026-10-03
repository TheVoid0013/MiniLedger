# MiniLedger

A DDD-style mini payment ledger in .NET 10: value objects, aggregates, domain services, and Clean Architecture.

Accounts can receive deposits, make withdrawals, transfer money to each other, and list their transaction history.

## Tech

.NET 10 · ASP.NET Core · EF Core + SQLite · MediatR

## Architecture

```text
src/
├── MiniLedger.Domain          Account (aggregate root), Transaction (entity),
│                              Money / Currency / AccountNumber (value objects),
│                              TransferService (domain service)
├── MiniLedger.Application     Commands, queries, handlers, transaction pipeline behavior
├── MiniLedger.Infrastructure  AppDbContext, EF configurations, EfTransactionManager
└── MiniLedger.Api             Controllers, exception handler (ProblemDetails)
```

Dependencies point inward: Api → Application → Domain, and Infrastructure → Application. The Domain references nothing.

**Design notes**

- Business rules live in the domain (`Account.Withdraw`, `TransferService`), never in controllers.
- Handlers use `DbContext` directly (no repository layer); commands are wrapped in a DB transaction by a mediator pipeline behavior.
- `Account` carries a version token for optimistic concurrency, so concurrent writers get a 409 instead of overdrawing.

## Run

```bash
dotnet run --project src/MiniLedger.Api
```

API at `http://localhost:5118`. The SQLite database (`ledger.db`) is created and migrated on startup.

## API

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/accounts` | Create account `{ owner, currency }` |
| GET | `/api/accounts/{id}` | Get account |
| GET | `/api/accounts/{id}/transactions` | Transaction history |
| POST | `/api/accounts/{id}/deposit` | Deposit `{ amount, currency }` |
| POST | `/api/accounts/{id}/withdraw` | Withdraw `{ amount, currency }` |
| POST | `/api/transfers` | Transfer `{ sourceAccountId, destinationAccountId, amount, currency }` |

**Errors:** `400` business rule or invalid input · `404` account not found · `409` concurrent update · `422` insufficient funds

## Quick try

```bash
curl -X POST http://localhost:5118/api/accounts \
  -H "Content-Type: application/json" \
  -d '{"owner":"Ram","currency":"NPR"}'
```
