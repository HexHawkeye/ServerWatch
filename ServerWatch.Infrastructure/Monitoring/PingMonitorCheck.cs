using System.Diagnostics;
using System.Net.NetworkInformation;
using ServerWatch.Core.Models;
using ServerWatch.Core.Monitoring;

namespace ServerWatch.Infrastructure.Monitoring;

public sealed class PingMonitorCheck : IMonitorCheck
{
    public string CheckType => "Ping";

    public async Task<MonitorCheckResult> ExecuteAsync(MonitorTarget target, CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var stopwatch = Stopwatch.StartNew();
            var reply = await ping.SendPingAsync(target.Host, target.TimeoutSeconds * 1000).WaitAsync(cancellationToken);
            stopwatch.Stop();
            return reply.Status == IPStatus.Success
                ? new(true, (int)stopwatch.ElapsedMilliseconds, $"Reply from {reply.Address}")
                : new(false, (int)stopwatch.ElapsedMilliseconds, $"Ping {reply.Status}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return new(false, null, ex.Message);
        }
    }
}
