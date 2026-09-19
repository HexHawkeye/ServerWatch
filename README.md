# ServerWatch

ServerWatch is a self-hosted infrastructure monitoring application built with ASP.NET Core .NET 10. It performs Ping, TCP, HTTP/HTTPS and SQL Server checks, records response history, suppresses transient failures and sends outage/recovery alerts.

## Phase 4 highlights

- Administrator authentication with secure, HTTP-only cookies
- SQLite for quick installs or SQL Server for production
- Docker and Docker Compose deployment with persistent data
- `/health` container health endpoint
- JSON structured console logging
- SQLite backup download from the Settings page
- GitHub Actions build, test and container pipeline
- Unit-tested failure-threshold and alert-transition policy
- Configuration through user secrets and environment variables
- All Phase 3 monitoring, alerting, charts and retention features

## First run in Visual Studio

1. Open `ServerWatch.sln`.
2. Set `ServerWatch.Web` as the startup project.
3. Press F5.
4. Sign in with `admin` / `change-me-now`.
5. Change the password hash before exposing the application to a network.

Create a SHA-256 password hash in PowerShell:

```powershell
$password = Read-Host -AsSecureString
$plain = [System.Net.NetworkCredential]::new('', $password).Password
$bytes = [Text.Encoding]::UTF8.GetBytes($plain)
[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLower()
```

Store it without editing tracked configuration:

```powershell
cd ServerWatch.Web
dotnet user-secrets set "Admin:PasswordSha256" "YOUR_HASH"
dotnet user-secrets set "Alerts:Smtp:Password" "YOUR_SMTP_PASSWORD"
```

## Docker

Create a `.env` file containing your hash:

```text
SERVERWATCH_ADMIN_PASSWORD_SHA256=your_hash_here
SERVERWATCH_SMTP_PASSWORD=optional_smtp_password
```

Then run:

```bash
docker compose up -d --build
```

Open `http://localhost:8080`. SQLite data is retained in the `serverwatch-data` volume.

## SQL Server mode

Set:

```text
DatabaseProvider=SqlServer
ConnectionStrings__ServerWatch=Server=SQL01;Database=ServerWatch;User Id=serverwatch;Password=...;TrustServerCertificate=true
```

SQLite remains the default. Existing Phase 3 SQLite databases are upgraded automatically. For an established SQL Server deployment, use controlled EF migrations before changing the schema.

## Configuration

Important environment variables:

| Variable | Purpose |
|---|---|
| `Admin__Username` | Administrator username |
| `Admin__PasswordSha256` | SHA-256 password hash |
| `DatabaseProvider` | `Sqlite` or `SqlServer` |
| `ConnectionStrings__ServerWatch` | Application database |
| `Alerts__Smtp__Enabled` | Enables email alerts |
| `Alerts__Smtp__Host` | SMTP hostname |
| `Alerts__Smtp__Password` | SMTP password |

Never commit real credentials, populated databases or `.env` files.

## Build and test

```bash
dotnet restore ServerWatch.sln
dotnet build ServerWatch.sln -c Release
dotnet test ServerWatch.Tests/ServerWatch.Tests.csproj -c Release
```

The CI workflow performs the same build and tests, then verifies the Docker image builds successfully.
