using Microsoft.AspNetCore.Mvc;using Microsoft.AspNetCore.Mvc.RazorPages;using Microsoft.Data.Sqlite;using Microsoft.EntityFrameworkCore;using ServerWatch.Infrastructure.Data;
namespace ServerWatch.Web.Pages;
public class SettingsModel(IConfiguration config,ServerWatchDbContext db):PageModel
{
 public bool SmtpEnabled=>config.GetValue<bool>("Alerts:Smtp:Enabled");public string DatabaseProvider=>config["DatabaseProvider"]??"Sqlite";
 public async Task<IActionResult> OnGetDownloadBackupAsync(){var configured=db.Database.GetDbConnection();if(configured is not SqliteConnection)return BadRequest("Direct backup download is available for SQLite only.");var temporary=Path.Combine(Path.GetTempPath(),$"serverwatch-{Guid.NewGuid():N}.db");try{await using var source=new SqliteConnection(configured.ConnectionString);await source.OpenAsync();await using var destination=new SqliteConnection($"Data Source={temporary}");await destination.OpenAsync();source.BackupDatabase(destination);await destination.CloseAsync();var bytes=await System.IO.File.ReadAllBytesAsync(temporary);return File(bytes,"application/vnd.sqlite3",$"serverwatch-{DateTime.UtcNow:yyyyMMdd-HHmm}.db");}finally{if(System.IO.File.Exists(temporary))System.IO.File.Delete(temporary);}}
}
