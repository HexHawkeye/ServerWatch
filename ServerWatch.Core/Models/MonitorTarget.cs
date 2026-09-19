namespace ServerWatch.Core.Models;
public class MonitorTarget
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Host { get; set; } = "";
    public string CheckType { get; set; } = "Ping";
    public int? Port { get; set; }
    public int IntervalSeconds { get; set; } = 60;
    public int TimeoutSeconds { get; set; } = 5;
    public string? ConnectionString { get; set; }
    public int FailureThreshold { get; set; } = 3;
    public bool AlertsEnabled { get; set; } = true;
    public string? AlertEmail { get; set; }
    public string? WebhookUrl { get; set; }
    public bool Enabled { get; set; } = true;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public ICollection<CheckResult> Results { get; set; } = new List<CheckResult>();
}
