using Microsoft.AspNetCore.Mvc.RazorPages;using Microsoft.EntityFrameworkCore;using ServerWatch.Core.Models;using ServerWatch.Infrastructure.Data;
namespace ServerWatch.Web.Pages;
public class HistoryModel(ServerWatchDbContext db):PageModel
{
 public MonitorTarget? Target{get;private set;}public List<CheckResult> Results{get;private set;}=[];public List<CheckResult> ChartResults=>Results.Where(x=>x.ResponseTimeMs.HasValue).Take(40).Reverse().ToList();public int ChartMax=>Math.Max(1,ChartResults.Count==0?1:ChartResults.Max(x=>x.ResponseTimeMs!.Value));public double UptimePercent{get;private set;}
 public async Task OnGetAsync(int id){Target=await db.MonitorTargets.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id);if(Target is null)return;Results=await db.CheckResults.AsNoTracking().Where(x=>x.MonitorTargetId==id).OrderByDescending(x=>x.CheckedUtc).Take(100).ToListAsync();UptimePercent=Results.Count==0?0:Results.Count(x=>x.IsSuccessful)*100d/Results.Count;}
}
