# ServerWatch

Self-hosted infrastructure monitoring built with ASP.NET Core .NET 10, Razor Pages, EF Core and SQLite.

## features

- Ping, TCP, HTTP/HTTPS and SQL Server checks
- Per-monitor schedules and timeouts
- Configurable consecutive-failure thresholds
- Healthy, Warning and Unhealthy states to reduce false alarms
- Failure and recovery notifications by email and/or JSON webhook
- Manual **Run now** checks, including paused monitors
- Full monitor editing and pause/resume controls
- Response-time chart and latest 100 results per monitor
- Thirty-day result retention with automatic cleanup
- Non-destructive upgrades from Phase 1 or Phase 2 databases

## Run

Open `ServerWatch.sln`, set `ServerWatch.Web` as the startup project and press F5, or run:

```powershell
cd ServerWatch.Web
dotnet restore
dotnet run
```

The app automatically creates or upgrades `ServerWatch.Web/serverwatch.db`.

## Alerts

Webhook notifications need only a URL entered on the monitor's Edit page. ServerWatch sends a JSON POST when the monitor becomes Unhealthy and again when it recovers.

For email, configure `Alerts:Smtp` in `ServerWatch.Web/appsettings.json`, then enter the recipient on the monitor's Edit page:

```json
"Alerts": {
  "Smtp": {
    "Enabled": true,
    "Host": "smtp.example.com",
    "Port": 587,
    "EnableSsl": true,
    "Username": "monitoring@example.com",
    "Password": "use-user-secrets-or-an-environment-variable",
    "FromAddress": "monitoring@example.com"
  }
}
```

For local development, keep passwords out of source control with .NET user secrets:

```powershell
cd ServerWatch.Web
dotnet user-secrets init
dotnet user-secrets set "Alerts:Smtp:Password" "your-password"
```

In deployment, use the environment variable `Alerts__Smtp__Password`.

## Failure behaviour

With a failure threshold of three:

1. The first and second failed checks show **Warning**.
2. The third failed check changes the monitor to **Unhealthy** and sends one failure alert.
3. Further failures do not repeatedly alert.
4. The first successful check returns it to **Healthy** and sends one recovery alert.

Results older than 30 days are removed automatically every six hours.
