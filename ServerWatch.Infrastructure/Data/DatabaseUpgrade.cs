using Microsoft.EntityFrameworkCore;

namespace ServerWatch.Infrastructure.Data;

public static class DatabaseUpgrade
{
    public static async Task ApplyAsync(ServerWatchDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await AddColumnIfMissingAsync(db, "MonitorTargets", "IntervalSeconds", "INTEGER NOT NULL DEFAULT 60");
        await AddColumnIfMissingAsync(db, "MonitorTargets", "TimeoutSeconds", "INTEGER NOT NULL DEFAULT 5");
        await AddColumnIfMissingAsync(db, "MonitorTargets", "ConnectionString", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "CheckResults", "StatusChanged", "INTEGER NOT NULL DEFAULT 0");
        await AddColumnIfMissingAsync(db, "MonitorTargets", "FailureThreshold", "INTEGER NOT NULL DEFAULT 3");
        await AddColumnIfMissingAsync(db, "MonitorTargets", "AlertsEnabled", "INTEGER NOT NULL DEFAULT 1");
        await AddColumnIfMissingAsync(db, "MonitorTargets", "AlertEmail", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "MonitorTargets", "WebhookUrl", "TEXT NULL");
        await AddColumnIfMissingAsync(db, "CheckResults", "IsSuccessful", "INTEGER NOT NULL DEFAULT 0");
        await AddColumnIfMissingAsync(db, "CheckResults", "Status", "TEXT NOT NULL DEFAULT 'Pending'");
        await AddColumnIfMissingAsync(db, "CheckResults", "ConsecutiveFailures", "INTEGER NOT NULL DEFAULT 0");
        await db.Database.ExecuteSqlRawAsync("UPDATE CheckResults SET IsSuccessful=IsHealthy, Status=CASE WHEN IsHealthy=1 THEN 'Healthy' ELSE 'Unhealthy' END WHERE Status='Pending'");
    }

    private static async Task AddColumnIfMissingAsync(DbContext db, string table, string column, string definition)
    {
        var connection = db.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open) await connection.OpenAsync();
        await using var inspect = connection.CreateCommand();
        inspect.CommandText = $"PRAGMA table_info(\"{table}\")";
        await using var reader = await inspect.ExecuteReaderAsync();
        var exists = false;
        while (await reader.ReadAsync())
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase)) exists = true;
        await reader.DisposeAsync();
        if (exists) return;
        await using var alter = connection.CreateCommand();
        alter.CommandText = $"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {definition}";
        await alter.ExecuteNonQueryAsync();
    }
}
