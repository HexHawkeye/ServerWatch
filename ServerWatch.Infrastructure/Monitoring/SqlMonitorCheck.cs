using System.Diagnostics;
using Microsoft.Data.SqlClient;
using ServerWatch.Core.Models;
using ServerWatch.Core.Monitoring;

namespace ServerWatch.Infrastructure.Monitoring;

public sealed class SqlMonitorCheck : IMonitorCheck
{
    public string CheckType => "SQL";

    public async Task<MonitorCheckResult> ExecuteAsync(MonitorTarget target, CancellationToken cancellationToken)
    {
        var connectionString = string.IsNullOrWhiteSpace(target.ConnectionString)
            ? $"Server={target.Host}{(target.Port is null ? "" : $",{target.Port}")};Integrated Security=true;TrustServerCertificate=true;Connection Timeout={target.TimeoutSeconds}"
            : target.ConnectionString;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(target.TimeoutSeconds));
            await using var connection = new SqlConnection(connectionString);
            var stopwatch = Stopwatch.StartNew();
            await connection.OpenAsync(timeout.Token);
            await using var command = new SqlCommand("SELECT 1", connection) { CommandTimeout = target.TimeoutSeconds };
            await command.ExecuteScalarAsync(timeout.Token);
            stopwatch.Stop();
            return new(true, (int)stopwatch.ElapsedMilliseconds, "SQL connection and query succeeded");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return new(false, null, ex is OperationCanceledException ? "SQL check timed out" : ex.Message);
        }
    }
}
