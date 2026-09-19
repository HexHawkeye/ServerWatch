using ServerWatch.Core.Models;
namespace ServerWatch.Core.Monitoring;
public interface IAlertSender
{
 Task SendFailureAsync(MonitorTarget target, CheckResult result, CancellationToken cancellationToken);
 Task SendRecoveryAsync(MonitorTarget target, CheckResult result, CancellationToken cancellationToken);
}
