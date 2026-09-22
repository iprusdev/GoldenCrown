# GoldenCrown

## Project description

GoldenCrown is an educational banking REST API built with ASP.NET Core and Entity Framework Core. It supports:

- user registration and login;
- token-based authentication with one active session per user;
- three currency accounts per user (USD, EUR, BYN) with independent balances;
- account deposits;
- money transfers between users;
- filtered and paginated transaction history;
- automatic removal of expired sessions every 10 minutes.

The application uses SQL Server for persistence and exposes Swagger UI in the Development environment.

Passwords are hashed with ASP.NET Core's `PasswordHasher<TUser>`. Passwords stored by an older version of the application are upgraded to the hashed format after the user's next successful login.

## Technology stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10
- SQL Server
- Swagger / OpenAPI

## CQRS architecture

Controllers send requests through MediatR. Each operation in `GoldenCrown/Features`
has its own request and handler. Handlers contain the business rules and use
`ApplicationDbContext` directly; the former user, account and finance services and
their interfaces have been removed.

| Type | Requests |
| --- | --- |
| Commands (write) | `UserRegisterCommand`, `UserLoginCommand`, `CreateAccountCommand`, `DepositCommand`, `TransferCommand`, `CleanupExpiredSessionsCommand` |
| Queries (read) | `GetAccountsQuery`, `GetBalanceQuery`, `GetTransactionHistoryQuery` |

Login is a command because it creates or updates a session and can upgrade a
password hash. All queries use `AsNoTracking()` and never save changes.
Registration saves the user and three zero-balance accounts together in one
`SaveChangesAsync` call. `SessionCleanupService` remains a background scheduler
and dispatches its cleanup command through MediatR.

Financial requests explicitly select a currency; existing routes remain available. Controllers validate incoming DTOs, while
handlers enforce business rules and propagate cancellation to EF Core.
Commands and queries share the existing SQL Server database.

### Tests

```powershell
dotnet test GoldenCrown.slnx
```

`GoldenCrown.Tests` exercises all nine operations through MediatR, including
rejected transfers, token rotation, read-only queries and cancellation.
Handler tests use EF Core InMemory. To also test migration, constraints and rollback
on a temporary SQL Server Express database:

```powershell
$env:GOLDENCROWN_SQL_TESTS = '1'
dotnet test GoldenCrown.slnx
```

The integration test creates and deletes only its own GUID-named database.

## Startup instructions

### Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) (`global.json` requests SDK `10.0.302`)
- SQL Server or SQL Server Express
- Entity Framework Core CLI tools

If `dotnet ef` is not installed, install it with:

```powershell
dotnet tool install --global dotnet-ef --version 10.*
```

### 1. Configure the database connection

The default connection string in `GoldenCrown/appsettings.json` uses a local SQL Server Express instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=GoldenCrownDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Change it if your SQL Server instance has a different name or authentication method. You can also override it without editing the file:

```powershell
$env:ConnectionStrings__DefaultConnection = "your-connection-string"
```

### 2. Restore packages and create the database

Run these commands from the repository root:

```powershell
dotnet restore GoldenCrown.slnx
dotnet ef database update --project GoldenCrown
```

The migration creates the `GoldenCrownDB` schema and inserts three sample users.

### 3. Start the API

HTTP profile:

```powershell
dotnet run --project GoldenCrown --launch-profile http
```

The API will be available at `http://localhost:5256`. Swagger UI is available at:

```text
http://localhost:5256/swagger
```

For HTTPS, trust the local development certificate and use the `https` profile:

```powershell
dotnet dev-certs https --trust
dotnet run --project GoldenCrown --launch-profile https
```

The HTTPS endpoint is `https://localhost:7082`.

## Authentication

Call the login endpoint to receive a token. Sessions are valid for one hour. Send the token with every `/api/Finance` request:

```http
Authorization: Bearer <token>
```

Expired sessions are removed in the background every 10 minutes. Logging in again replaces the user's previous session.

## API requests

The examples below use `http://localhost:5256` as the base URL. JSON property names are case-insensitive when sent to ASP.NET Core.

### Register a user

`POST /api/user/Register`

```bash
curl -X POST "http://localhost:5256/api/user/Register" \
  -H "Content-Type: application/json" \
  -d '{
    "login": "john123",
    "name": "John Smith",
    "password": "secret123"
  }'
```

Validation rules:

- `login`: required, 3-50 characters;
- `name`: required, maximum 100 characters;
- `password`: required, 6-100 characters.

Successful response: `200 OK` with an empty body. Invalid input or a registration error returns `400 Bad Request`.

### Log in

`POST /api/user/Login`

```bash
curl -X POST "http://localhost:5256/api/user/Login" \
  -H "Content-Type: application/json" \
  -d '{
    "login": "john123",
    "password": "secret123"
  }'
```

Successful response (`200 OK`):

```json
{
  "token": "03221572-0961-461d-b984-915d5de74a40"
}
```

Invalid input returns `400 Bad Request`; invalid credentials return `401 Unauthorized`.

### Get all accounts

`GET /api/Finance/accounts`

Returns only the authenticated user's accounts:

```json
[
  { "id": 1, "currency": "BYN", "balance": 0 },
  { "id": 2, "currency": "USD", "balance": 100 },
  { "id": 3, "currency": "EUR", "balance": 0 }
]
```

### Get the current balance

`GET /api/Finance/balance?currency=USD`

```bash
curl "http://localhost:5256/api/Finance/balance?currency=USD" \
  -H "Authorization: Bearer <token>"
```

Successful response (`200 OK`):

```json
{
  "currency": "USD",
  "balance": 1250.50
}
```

### Deposit money

`POST /api/Finance/deposit`

```bash
curl -X POST "http://localhost:5256/api/Finance/deposit" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "currency": "USD",
    "amount": 500.00
  }'
```

`currency` is required and must be `USD`, `EUR` or `BYN`; `amount` must be greater than zero. A successful request returns `200 OK` with an empty body.

### Transfer money

`POST /api/Finance/transfer`

```bash
curl -X POST "http://localhost:5256/api/Finance/transfer" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "receiverLogin": "alice123",
    "currency": "USD",
    "amount": 100.00
  }'
```

Both accounts use the specified `currency`; currency conversion is not supported. `receiverLogin` is required; `currency` must be `USD`, `EUR` or `BYN`; `amount` must be greater than zero. A successful request returns `200 OK`. The API returns `400 Bad Request` when the receiver does not exist, the sender has insufficient funds, or the sender tries to transfer money to the same account.

### Get transaction history

`GET /api/Finance/history`

```bash
curl "http://localhost:5256/api/Finance/history?from=2026-01-01T00:00:00Z&to=2026-12-31T23:59:59Z&offset=0&limit=20" \
  -H "Authorization: Bearer <token>"
```

Query parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `currency` | USD / EUR / BYN | No | Filter by currency; omitted means all currencies. |
| `from` | ISO 8601 date-time | No | Include transactions on or after this time. |
| `to` | ISO 8601 date-time | No | Include transactions on or before this time. |
| `offset` | integer | Yes | Number of records to skip; must be at least `0`. |
| `limit` | integer | Yes | Maximum records to return; must be at least `1`. |

Successful response (`200 OK`):

```json
[
  {
    "senderName": "John Smith",
    "receiverName": "Alice Brown",
    "currency": "USD",
    "amount": 100.00,
    "date": "2026-09-04T12:30:00+03:00"
  }
]
```

Results are ordered from newest to oldest. An invalid date range, negative offset, or non-positive limit returns `400 Bad Request`.

## Existing data migration

`MultiCurrencyAccounts` treats existing balances and transaction history as BYN.
It creates missing USD, EUR and BYN accounts with zero balances and preserves
existing money without conversion. The application applies pending migrations
at startup; alternatively run `dotnet ef database update --project GoldenCrown`.
Rollback is blocked while any USD/EUR balance is nonzero or USD/EUR history exists,
so reverting cannot silently discard money or reinterpret another currency.

## Database structure

### `Users`

| Column | SQL type | Constraints |
| --- | --- | --- |
| `Id` | `int` | Primary key, identity |
| `Login` | `nvarchar(50)` | Required, unique |
| `Name` | `nvarchar(100)` | Required |
| `PasswordHash` | `nvarchar(500)` | Required |

### `Accounts`

| Column | SQL type | Constraints |
| --- | --- | --- |
| `Id` | `int` | Primary key, identity |
| `UserId` | `int` | Required, foreign key to `Users.Id` |
| `Currency` | `nvarchar(3)` | Required; USD, EUR or BYN; unique together with UserId |
| `Balance` | `decimal(18,2)` | Required |

Registration creates exactly three accounts per user, one per supported currency. Deleting a user also deletes the related accounts.

### `Sessions`

| Column | SQL type | Constraints |
| --- | --- | --- |
| `UserId` | `int` | Primary key, foreign key to `Users.Id` |
| `Token` | `nvarchar(100)` | Required, unique |
| `ExpiresAt` | `datetimeoffset` | Required |

Using `UserId` as the primary key enforces one active session per user. Deleting a user also deletes the related session.

### `Transactions`

| Column | SQL type | Constraints |
| --- | --- | --- |
| `Id` | `bigint` | Primary key, identity |
| `SenderId` | `int` | Nullable foreign key to `Users.Id` |
| `ReceiverId` | `int` | Required foreign key to `Users.Id` |
| `Date` | `datetimeoffset` | Required |
| `Currency` | `nvarchar(3)` | Required; USD, EUR or BYN |
| `Amount` | `decimal(18,2)` | Required |

`SenderId` and `ReceiverId` use restricted delete behavior so users referenced by transactions cannot be deleted automatically.

### Relationships

```text
Users 1 ─── many Accounts (unique UserId + Currency)
Users 1 ─── 0..1 Sessions
Users 1 ─── many Transactions (SenderId)
Users 1 ─── many Transactions (ReceiverId)
```

### Seed data

The `SeedData` migration creates these users:

| Id | Login | Name | Login password |
| --- | --- | --- | --- |
| 1 | `testuser1` | Test User 1 | `seed-test-hash-1` |
| 2 | `testuser2` | Test User 2 | `seed-test-hash-2` |
| 3 | `testuser3` | Test User 3 | `seed-test-hash-3` |

The MultiCurrencyAccounts migration also creates USD, EUR and BYN accounts for all seeded users.
