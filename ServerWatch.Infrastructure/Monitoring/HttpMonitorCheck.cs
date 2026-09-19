using System.Diagnostics;
using ServerWatch.Core.Models;
using ServerWatch.Core.Monitoring;

namespace ServerWatch.Infrastructure.Monitoring;

public sealed class HttpMonitorCheck(IHttpClientFactory clientFactory) : IMonitorCheck
{
    public string CheckType => "HTTP";

    public async Task<MonitorCheckResult> ExecuteAsync(MonitorTarget target, CancellationToken cancellationToken)
    {
        var address = target.Host.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                      target.Host.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            ? target.Host
            : $"{(target.Port == 443 ? "https" : "http")}://{target.Host}{(target.Port is null ? "" : $":{target.Port}")}";
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(target.TimeoutSeconds));
            var stopwatch = Stopwatch.StartNew();
            using var response = await clientFactory.CreateClient("monitor").GetAsync(address, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            stopwatch.Stop();
            return new(response.IsSuccessStatusCode, (int)stopwatch.ElapsedMilliseconds,
                $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return new(false, null, ex is OperationCanceledException ? "Request timed out" : ex.Message);
        }
    }
}
