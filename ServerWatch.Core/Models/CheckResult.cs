namespace ServerWatch.Core.Models;
public class CheckResult
{
    public long Id { get; set; }
    public int MonitorTargetId { get; set; }
    public MonitorTarget? MonitorTarget { get; set; }
    public bool IsHealthy { get; set; }
    public bool IsSuccessful { get; set; }
    public string Status { get; set; } = "Pending";
    public int ConsecutiveFailures { get; set; }
    public int? ResponseTimeMs { get; set; }
    public string? Message { get; set; }
    public bool StatusChanged { get; set; }
    public DateTime CheckedUtc { get; set; } = DateTime.UtcNow;
}
