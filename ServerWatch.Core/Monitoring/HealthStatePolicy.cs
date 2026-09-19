namespace ServerWatch.Core.Monitoring;
public static class HealthStatePolicy
{
 public static (string Status,int Failures) Evaluate(bool success,int previousFailures,int threshold)=>success?("Healthy",0):(previousFailures+1>=Math.Max(1,threshold)?"Unhealthy":"Warning",previousFailures+1);
 public static bool IsFailureAlert(string? previous,string current)=>previous is not null&&previous!="Unhealthy"&&current=="Unhealthy";
 public static bool IsRecoveryAlert(string? previous,string current)=>previous=="Unhealthy"&&current=="Healthy";
}
