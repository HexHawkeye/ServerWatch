using System.Net;using System.Net.Mail;using System.Net.Http.Json;using Microsoft.Extensions.Logging;using Microsoft.Extensions.Options;using ServerWatch.Core.Models;using ServerWatch.Core.Monitoring;
namespace ServerWatch.Infrastructure.Monitoring;
public sealed class AlertSender(IHttpClientFactory clients,IOptions<AlertOptions> options,ILogger<AlertSender> logger):IAlertSender
{
 public Task SendFailureAsync(MonitorTarget target,CheckResult result,CancellationToken ct)=>SendAsync(target,result,"DOWN",$"ServerWatch detected {result.ConsecutiveFailures} consecutive failures.",ct);
 public Task SendRecoveryAsync(MonitorTarget target,CheckResult result,CancellationToken ct)=>SendAsync(target,result,"RECOVERED","ServerWatch detected that the monitor has recovered.",ct);
 private async Task SendAsync(MonitorTarget target,CheckResult result,string state,string summary,CancellationToken ct)
 {
  if(!target.AlertsEnabled)return;var subject=$"[ServerWatch] {target.Name} {state}";var body=$"{summary}\n\nMonitor: {target.Name}\nTarget: {target.Host}\nCheck: {target.CheckType}\nResponse: {result.ResponseTimeMs?.ToString()??"n/a"} ms\nMessage: {result.Message}\nUTC: {result.CheckedUtc:u}";
  if(!string.IsNullOrWhiteSpace(target.WebhookUrl))try{using var response=await clients.CreateClient("alerts").PostAsJsonAsync(target.WebhookUrl,new{source="ServerWatch",monitor=target.Name,state,stateChanged=true,result.Message,result.ResponseTimeMs,checkedUtc=result.CheckedUtc},ct);response.EnsureSuccessStatusCode();}catch(Exception ex){logger.LogError(ex,"Webhook alert failed for {Monitor}",target.Name);}
  var smtp=options.Value.Smtp;if(smtp.Enabled&&!string.IsNullOrWhiteSpace(target.AlertEmail))try{using var message=new MailMessage(smtp.FromAddress,target.AlertEmail,subject,body);using var client=new SmtpClient(smtp.Host,smtp.Port){EnableSsl=smtp.EnableSsl};if(!string.IsNullOrWhiteSpace(smtp.Username))client.Credentials=new NetworkCredential(smtp.Username,smtp.Password);await client.SendMailAsync(message).WaitAsync(ct);}catch(Exception ex){logger.LogError(ex,"Email alert failed for {Monitor}",target.Name);}
 }
}
