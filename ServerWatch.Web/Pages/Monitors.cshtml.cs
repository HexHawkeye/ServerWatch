using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Mvc.RazorPages;using Microsoft.EntityFrameworkCore;using ServerWatch.Core.Models;using ServerWatch.Core.Monitoring;using ServerWatch.Infrastructure.Data;
namespace ServerWatch.Web.Pages;
public class MonitorsModel(ServerWatchDbContext db,MonitorRunQueue runQueue):PageModel
{
 public List<MonitorTarget> Targets{get;set;}=[];[BindProperty]public MonitorTarget Input{get;set;}=new();public async Task OnGetAsync()=>Targets=await db.MonitorTargets.OrderBy(x=>x.Name).ToListAsync();
 public async Task<IActionResult> OnPostAsync(){Input.Name=Input.Name.Trim();Input.Host=Input.Host.Trim();Input.CheckType=Input.CheckType.Trim().ToUpperInvariant() switch{"PING"=>"Ping","TCP"=>"TCP","HTTP"=>"HTTP","SQL"=>"SQL",_=>""};if(string.IsNullOrWhiteSpace(Input.Name)||string.IsNullOrWhiteSpace(Input.Host)||string.IsNullOrWhiteSpace(Input.CheckType))return RedirectToPage();if(Input.CheckType=="SQL"&&Input.Port is null)Input.Port=1433;Input.IntervalSeconds=Math.Clamp(Input.IntervalSeconds,10,86400);Input.TimeoutSeconds=Math.Clamp(Input.TimeoutSeconds,1,120);Input.FailureThreshold=Math.Clamp(Input.FailureThreshold,1,20);Input.Id=0;Input.CreatedUtc=DateTime.UtcNow;db.MonitorTargets.Add(Input);await db.SaveChangesAsync();return RedirectToPage();}
 public async Task<IActionResult> OnPostToggleAsync(int id){var row=await db.MonitorTargets.FindAsync(id);if(row!=null){row.Enabled=!row.Enabled;await db.SaveChangesAsync();}return RedirectToPage();}
 public IActionResult OnPostRun(int id){runQueue.Enqueue(id);TempData["Message"]="Check queued and will run within two seconds.";return RedirectToPage();}
 public async Task<IActionResult> OnPostDeleteAsync(int id){var row=await db.MonitorTargets.FindAsync(id);if(row!=null){db.Remove(row);await db.SaveChangesAsync();}return RedirectToPage();}
}
