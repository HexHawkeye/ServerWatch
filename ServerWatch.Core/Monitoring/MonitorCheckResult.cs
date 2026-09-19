namespace ServerWatch.Core.Monitoring;

public sealed record MonitorCheckResult(bool IsHealthy, int? ResponseTimeMs, string Message);
