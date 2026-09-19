using Microsoft.EntityFrameworkCore;
using ServerWatch.Core.Models;
namespace ServerWatch.Infrastructure.Data;
public class ServerWatchDbContext(DbContextOptions<ServerWatchDbContext> options) : DbContext(options)
{
    public DbSet<MonitorTarget> MonitorTargets => Set<MonitorTarget>();
    public DbSet<CheckResult> CheckResults => Set<CheckResult>();
    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<MonitorTarget>().Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Entity<MonitorTarget>().Property(x => x.Host).HasMaxLength(255).IsRequired();
        b.Entity<MonitorTarget>().Property(x => x.ConnectionString).HasMaxLength(2000);
        b.Entity<MonitorTarget>().Property(x => x.AlertEmail).HasMaxLength(320);
        b.Entity<MonitorTarget>().Property(x => x.WebhookUrl).HasMaxLength(2000);
        b.Entity<CheckResult>().Property(x => x.Message).HasMaxLength(500);
        b.Entity<CheckResult>().Property(x => x.Status).HasMaxLength(20);
        b.Entity<CheckResult>().HasOne(x => x.MonitorTarget).WithMany(x => x.Results).HasForeignKey(x => x.MonitorTargetId).OnDelete(DeleteBehavior.Cascade);
    }
}
