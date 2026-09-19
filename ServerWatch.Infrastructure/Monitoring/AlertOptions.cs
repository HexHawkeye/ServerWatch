namespace ServerWatch.Infrastructure.Monitoring;
public sealed class AlertOptions
{
 public SmtpOptions Smtp {get;set;}=new();
 public sealed class SmtpOptions
 {
  public bool Enabled{get;set;} public string Host{get;set;}=""; public int Port{get;set;}=587;
  public bool EnableSsl{get;set;}=true; public string Username{get;set;}=""; public string Password{get;set;}="";
  public string FromAddress{get;set;}="";
 }
}
