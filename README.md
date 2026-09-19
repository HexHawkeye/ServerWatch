# ServerWatch

[![Build](https://github.com/HexHawkeye/ServerWatch/actions/workflows/ci.yml/badge.svg)](https://github.com/HexHawkeye/ServerWatch/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4)
![Docker](https://img.shields.io/badge/Docker-ready-2496ED)

ServerWatch is a self-hosted infrastructure monitoring application built with ASP.NET Core and .NET 10. It performs Ping, TCP, HTTP/HTTPS, and SQL Server checks, records response history, suppresses transient failures, and sends outage and recovery alerts.

## Phase 4 highlights

- Administrator authentication with secure, HTTP-only cookies
- SQLite for quick installations or SQL Server for production deployments
- Docker and Docker Compose deployment with persistent data
- `/health` container health endpoint
- JSON structured console logging
- SQLite backup download from the Settings page
- GitHub Actions build, test, and container pipeline
- Unit-tested failure-threshold and alert-transition policy
- Configuration through user secrets and environment variables
- All Phase 3 monitoring, alerting, charting, and retention features

## Features

### Monitoring

- Ping availability checks
- TCP port checks
- HTTP and HTTPS endpoint checks
- SQL Server connectivity checks
- Background monitoring worker
- Manual **Run Now** checks
- Configurable and pausable monitors

### Reliability and alerting

- Healthy, Warning, and Unhealthy states
- Failure thresholds that suppress transient faults
- Outage and recovery notifications
- Email alerts through SMTP
- Webhook alerts
- Tested health-state and alert-transition rules

### History and administration

- Response-time and status-history charts
- Thirty-day history retention
- Administrator sign-in and sign-out
- SQLite database backup download
- Structured JSON logging

## Technology

- ASP.NET Core Razor Pages
- .NET 10
- Entity Framework Core
- SQLite or Microsoft SQL Server
- xUnit
- Docker and Docker Compose
- GitHub Actions

## Solution structure

| Project | Purpose |
|---|---|
| `ServerWatch.Core` | Domain models, monitoring contracts, and health-state policy |
| `ServerWatch.Infrastructure` | Database access, monitoring implementations, background processing, and alerts |
| `ServerWatch.Web` | Razor Pages user interface, authentication, configuration, and health endpoint |
| `ServerWatch.Tests` | Unit tests for monitoring and alert-transition behavior |

## First run in Visual Studio

1. Open `ServerWatch.sln`.
2. Set `ServerWatch.Web` as the startup project.
3. Press **F5**.
4. Sign in with `admin` / `change-me-now`.
5. Change the password hash before exposing the application to a network.

### Create an administrator password hash

Run the following in PowerShell:

```powershell
$password = Read-Host -AsSecureString
$plain = [System.Net.NetworkCredential]::new('', $password).Password
$bytes = [Text.Encoding]::UTF8.GetBytes($plain)
[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLower()
```

Store the resulting hash and SMTP password with .NET user secrets so they are not written to tracked configuration:

```powershell
cd ServerWatch.Web
dotnet user-secrets set "Admin:PasswordSha256" "YOUR_HASH"
dotnet user-secrets set "Alerts:Smtp:Password" "YOUR_SMTP_PASSWORD"
```

## Docker

Create a `.env` file in the repository root:

```text
SERVERWATCH_ADMIN_PASSWORD_SHA256=your_hash_here
SERVERWATCH_SMTP_PASSWORD=optional_smtp_password
```

Build and start the application:

```bash
docker compose up -d --build
```

Open [http://localhost:8080](http://localhost:8080). SQLite data is retained in the `serverwatch-data` volume.

To stop the containers without deleting the persistent volume:

```bash
docker compose down
```

## SQL Server mode

SQLite is the default database provider. To use SQL Server, configure these values through environment variables or another secure configuration source:

```text
DatabaseProvider=SqlServer
ConnectionStrings__ServerWatch=Server=SQL01;Database=ServerWatch;User Id=serverwatch;Password=...;TrustServerCertificate=true
```

Existing Phase 3 SQLite databases are upgraded automatically. For an established SQL Server deployment, use controlled Entity Framework migrations before changing the schema.

## Configuration

Important environment variables:

| Variable | Purpose |
|---|---|
| `Admin__Username` | Administrator username |
| `Admin__PasswordSha256` | SHA-256 administrator password hash |
| `DatabaseProvider` | Database provider: `Sqlite` or `SqlServer` |
| `ConnectionStrings__ServerWatch` | Application database connection string |
| `Alerts__Smtp__Enabled` | Enables email alerts |
| `Alerts__Smtp__Host` | SMTP server hostname |
| `Alerts__Smtp__Password` | SMTP account password |

Additional non-secret settings can be configured in `appsettings.json`. Use user secrets, environment variables, or a secret manager for credentials.

## Build and test

From the repository root:

```bash
dotnet restore ServerWatch.sln
dotnet build ServerWatch.sln -c Release
dotnet test ServerWatch.Tests/ServerWatch.Tests.csproj -c Release
```

The GitHub Actions workflow performs the same build and tests, then verifies that the Docker image builds successfully.

## Security

- Replace the default development password before network exposure.
- Never commit real credentials, populated databases, or `.env` files.
- Use HTTPS through a trusted reverse proxy for production deployments.
- Restrict access to the Settings page and downloaded database backups.
- Store production secrets in environment variables or a dedicated secret manager.


