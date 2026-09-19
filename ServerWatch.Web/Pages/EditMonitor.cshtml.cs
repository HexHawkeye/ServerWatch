using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Mvc.RazorPages;using ServerWatch.Core.Models;using ServerWatch.Infrastructure.Data;
namespace ServerWatch.Web.Pages;
public class EditMonitorModel(ServerWatchDbContext db):PageModel
{
 [BindProperty]public MonitorTarget Input{get;set;}=new();
 public async Task<IActionResult> OnGetAsync(int id){var row=await db.MonitorTargets.FindAsync(id);if(row is null)return RedirectToPage("/Monitors");Input=row;Input.ConnectionString="";return Page();}
 public async Task<IActionResult> OnPostAsync(){var row=await db.MonitorTargets.FindAsync(Input.Id);if(row is null)return RedirectToPage("/Monitors");row.Name=Input.Name.Trim();row.Host=Input.Host.Trim();row.CheckType=Input.CheckType;row.Port=Input.Port;row.IntervalSeconds=Math.Clamp(Input.IntervalSeconds,10,86400);row.TimeoutSeconds=Math.Clamp(Input.TimeoutSeconds,1,120);row.FailureThreshold=Math.Clamp(Input.FailureThreshold,1,20);row.AlertsEnabled=Input.AlertsEnabled;row.AlertEmail=string.IsNullOrWhiteSpace(Input.AlertEmail)?null:Input.AlertEmail.Trim();row.WebhookUrl=string.IsNullOrWhiteSpace(Input.WebhookUrl)?null:Input.WebhookUrl.Trim();if(!string.IsNullOrWhiteSpace(Input.ConnectionString))row.ConnectionString=Input.ConnectionString;await db.SaveChangesAsync();return RedirectToPage("/Monitors");}
}
