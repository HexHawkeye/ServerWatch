using Microsoft.EntityFrameworkCore;
using ServerWatch.Core.Models;
using ServerWatch.Core.Monitoring;
using ServerWatch.Infrastructure.Data;
using ServerWatch.Infrastructure.Monitoring;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ServerWatchDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("ServerWatch") ?? "Data Source=serverwatch.db"));
builder.Services.AddHttpClient("monitor", client => client.DefaultRequestHeaders.UserAgent.ParseAdd("ServerWatch/3.0"));
builder.Services.AddHttpClient("alerts", client => client.DefaultRequestHeaders.UserAgent.ParseAdd("ServerWatch/3.0"));
builder.Services.Configure<AlertOptions>(builder.Configuration.GetSection("Alerts"));
builder.Services.AddSingleton<MonitorRunQueue>();
builder.Services.AddSingleton<IAlertSender, AlertSender>();
builder.Services.AddSingleton<IMonitorCheck, PingMonitorCheck>();
builder.Services.AddSingleton<IMonitorCheck, TcpMonitorCheck>();
builder.Services.AddSingleton<IMonitorCheck, HttpMonitorCheck>();
builder.Services.AddSingleton<IMonitorCheck, SqlMonitorCheck>();
builder.Services.AddHostedService<MonitoringWorker>();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServerWatchDbContext>();
    await DatabaseUpgrade.ApplyAsync(db);
    if (!db.MonitorTargets.Any())
    {
        db.MonitorTargets.AddRange(
            new MonitorTarget { Name="OPENAI-WEB", Host="https://openai.com", CheckType="HTTP", IntervalSeconds=60 },
            new MonitorTarget { Name="CLOUDFLARE-DNS", Host="1.1.1.1", CheckType="Ping", IntervalSeconds=60 },
            new MonitorTarget { Name="HTTPS-PORT", Host="example.com", CheckType="TCP", Port=443, IntervalSeconds=60 });
        await db.SaveChangesAsync();
    }
}
app.Run();
