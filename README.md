# GoldenCrown

## Project description

GoldenCrown is an educational banking REST API built with ASP.NET Core and Entity Framework Core. It supports:

- user registration and login;
- token-based authentication with one active session per user;
- account balance lookup;
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

### Get the current balance

`GET /api/Finance`

```bash
curl "http://localhost:5256/api/Finance" \
  -H "Authorization: Bearer <token>"
```

Successful response (`200 OK`):

```json
{
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
    "amount": 500.00
  }'
```

`amount` must be greater than zero. A successful request returns `200 OK` with an empty body.

### Transfer money

`POST /api/Finance/transfer`

```bash
curl -X POST "http://localhost:5256/api/Finance/transfer" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "receiverLogin": "alice123",
    "amount": 100.00
  }'
```

`receiverLogin` is required and `amount` must be greater than zero. A successful request returns `200 OK`. The API returns `400 Bad Request` when the receiver does not exist, the sender has insufficient funds, or the sender tries to transfer money to the same account.

### Get transaction history

`GET /api/Finance/history`

```bash
curl "http://localhost:5256/api/Finance/history?from=2026-01-01T00:00:00Z&to=2026-12-31T23:59:59Z&offset=0&limit=20" \
  -H "Authorization: Bearer <token>"
```

Query parameters:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
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
    "amount": 100.00,
    "date": "2026-09-04T12:30:00+03:00"
  }
]
```

Results are ordered from newest to oldest. An invalid date range, negative offset, or non-positive limit returns `400 Bad Request`.

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
| `UserId` | `int` | Required, unique, foreign key to `Users.Id` |
| `Balance` | `decimal(18,2)` | Required |

Each user has one account. Deleting a user also deletes the related account.

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
| `Amount` | `decimal(18,2)` | Required |

`SenderId` and `ReceiverId` use restricted delete behavior so users referenced by transactions cannot be deleted automatically.

### Relationships

```text
Users 1 ─── 1 Accounts
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

The migration seeds only user rows; it does not create accounts for these users.
