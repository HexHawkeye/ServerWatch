using Microsoft.EntityFrameworkCore;using Microsoft.Extensions.DependencyInjection;using Microsoft.Extensions.Hosting;using Microsoft.Extensions.Logging;using ServerWatch.Core.Models;using ServerWatch.Core.Monitoring;using ServerWatch.Infrastructure.Data;
namespace ServerWatch.Infrastructure.Monitoring;
public sealed class MonitoringWorker(IServiceScopeFactory scopeFactory,IEnumerable<IMonitorCheck> checks,MonitorRunQueue runQueue,IAlertSender alerts,ILogger<MonitoringWorker> logger):BackgroundService
{
 private readonly Dictionary<string,IMonitorCheck> _checks=checks.ToDictionary(x=>x.CheckType,StringComparer.OrdinalIgnoreCase);private DateTime _nextCleanupUtc=DateTime.MinValue;
 protected override async Task ExecuteAsync(CancellationToken stoppingToken){await Task.Delay(TimeSpan.FromSeconds(2),stoppingToken);using var timer=new PeriodicTimer(TimeSpan.FromSeconds(2));do{try{await RunDueChecksAsync(stoppingToken);await CleanupAsync(stoppingToken);}catch(OperationCanceledException)when(stoppingToken.IsCancellationRequested){break;}catch(Exception ex){logger.LogError(ex,"Monitoring cycle failed");}}while(await timer.WaitForNextTickAsync(stoppingToken));}
 private async Task RunDueChecksAsync(CancellationToken ct)
 {
  using var scope=scopeFactory.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ServerWatchDbContext>();var targets=(await db.MonitorTargets.AsNoTracking().ToListAsync(ct)).Where(x=>x.Enabled||runQueue.IsQueued(x.Id)).ToList();
  foreach(var target in targets)
  {
   var previous=await db.CheckResults.AsNoTracking().Where(x=>x.MonitorTargetId==target.Id).OrderByDescending(x=>x.CheckedUtc).FirstOrDefaultAsync(ct);var forced=runQueue.Take(target.Id);
   if(!forced&&previous is not null&&previous.CheckedUtc.AddSeconds(Math.Max(10,target.IntervalSeconds))>DateTime.UtcNow)continue;
   if(!_checks.TryGetValue(target.CheckType,out var check)){logger.LogWarning("Unknown check type {CheckType} for {Monitor}",target.CheckType,target.Name);continue;}
   var outcome=await check.ExecuteAsync(target,ct);var (status,failures)=HealthStatePolicy.Evaluate(outcome.IsHealthy,previous?.ConsecutiveFailures??0,target.FailureThreshold);var previousStatus=previous?.Status;if(string.IsNullOrWhiteSpace(previousStatus)&&previous is not null)previousStatus=previous.IsHealthy?"Healthy":"Unhealthy";
   var result=new CheckResult{MonitorTargetId=target.Id,IsSuccessful=outcome.IsHealthy,IsHealthy=status=="Healthy",Status=status,ConsecutiveFailures=failures,ResponseTimeMs=outcome.ResponseTimeMs,Message=outcome.Message.Length>500?outcome.Message[..500]:outcome.Message,CheckedUtc=DateTime.UtcNow,StatusChanged=previous is null||!string.Equals(previousStatus,status,StringComparison.OrdinalIgnoreCase)};
   db.CheckResults.Add(result);await db.SaveChangesAsync(ct);
   if(HealthStatePolicy.IsFailureAlert(previousStatus,status))await alerts.SendFailureAsync(target,result,ct);else if(HealthStatePolicy.IsRecoveryAlert(previousStatus,status))await alerts.SendRecoveryAsync(target,result,ct);
   logger.LogInformation("{Monitor}: {Status} ({ResponseTime} ms){Forced}",target.Name,status,outcome.ResponseTimeMs,forced?" [manual]":"");
  }
 }
 private async Task CleanupAsync(CancellationToken ct){if(DateTime.UtcNow<_nextCleanupUtc)return;_nextCleanupUtc=DateTime.UtcNow.AddHours(6);using var scope=scopeFactory.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ServerWatchDbContext>();var cutoff=DateTime.UtcNow.AddDays(-30);var deleted=await db.CheckResults.Where(x=>x.CheckedUtc<cutoff).ExecuteDeleteAsync(ct);if(deleted>0)logger.LogInformation("Deleted {Count} expired check results",deleted);}
}
