# Architecture

```mermaid
flowchart TD
    UI[Razor Pages UI] --> DB[(SQLite or SQL Server)]
    Worker[Monitoring worker] --> Checks[Ping / TCP / HTTP / SQL]
    Worker --> DB
    Worker --> Alerts[Email / webhook alerts]
    Health[/health endpoint] --> Host[Container or host platform]
```

`ServerWatch.Core` contains domain models and monitoring policy. `ServerWatch.Infrastructure` contains EF Core persistence, check implementations, scheduling and alert delivery. `ServerWatch.Web` owns authentication, configuration and the Razor Pages interface. `ServerWatch.Tests` verifies threshold and alert-transition behaviour.

The worker executes enabled monitors independently, persists every result and promotes repeated failures from Warning to Unhealthy. Alerts are emitted only on state transitions, preventing repeated notifications during a continuing outage.
