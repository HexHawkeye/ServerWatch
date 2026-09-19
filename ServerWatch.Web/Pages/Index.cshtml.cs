using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ServerWatch.Core.Models;
using ServerWatch.Infrastructure.Data;
namespace ServerWatch.Web.Pages;
public class IndexModel(ServerWatchDbContext db) : PageModel
{
 public List<DashboardMonitor> Monitors {get;private set;}=[]; public int IncidentCount {get;private set;}
 public int HealthyCount=>Monitors.Count(x=>x.Latest?.Status=="Healthy");public int WarningCount=>Monitors.Count(x=>x.Latest?.Status=="Warning");public int UnhealthyCount=>Monitors.Count(x=>x.Latest?.Status=="Unhealthy");
 public async Task OnGetAsync(){var targets=await db.MonitorTargets.AsNoTracking().OrderBy(x=>x.Name).ToListAsync();var ids=targets.Select(x=>x.Id).ToList();var results=await db.CheckResults.AsNoTracking().Where(x=>ids.Contains(x.MonitorTargetId)).OrderByDescending(x=>x.CheckedUtc).ToListAsync();Monitors=targets.Select(x=>new DashboardMonitor(x,results.FirstOrDefault(r=>r.MonitorTargetId==x.Id))).ToList();var since=DateTime.UtcNow.AddHours(-24);IncidentCount=results.Count(x=>x.Status=="Unhealthy"&&x.StatusChanged&&x.CheckedUtc>=since);}
 public sealed record DashboardMonitor(MonitorTarget Target,CheckResult? Latest);
}
