using System.Diagnostics;
using System.Net.Sockets;
using ServerWatch.Core.Models;
using ServerWatch.Core.Monitoring;

namespace ServerWatch.Infrastructure.Monitoring;

public sealed class TcpMonitorCheck : IMonitorCheck
{
    public string CheckType => "TCP";

    public async Task<MonitorCheckResult> ExecuteAsync(MonitorTarget target, CancellationToken cancellationToken)
    {
        if (target.Port is null) return new(false, null, "TCP checks require a port");
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(target.TimeoutSeconds));
            using var client = new TcpClient();
            var stopwatch = Stopwatch.StartNew();
            await client.ConnectAsync(target.Host, target.Port.Value, timeout.Token);
            stopwatch.Stop();
            return new(true, (int)stopwatch.ElapsedMilliseconds, $"Port {target.Port} accepted a connection");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return new(false, null, ex is OperationCanceledException ? "Connection timed out" : ex.Message);
        }
    }
}
