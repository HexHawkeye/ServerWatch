using ServerWatch.Core.Models;

namespace ServerWatch.Core.Monitoring;

public interface IMonitorCheck
{
    string CheckType { get; }
    Task<MonitorCheckResult> ExecuteAsync(MonitorTarget target, CancellationToken cancellationToken);
}
