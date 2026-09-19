using System.Collections.Concurrent;
namespace ServerWatch.Core.Monitoring;
public sealed class MonitorRunQueue
{
 private readonly ConcurrentDictionary<int,byte> _ids=new();
 public void Enqueue(int id)=>_ids.TryAdd(id,0);
 public bool IsQueued(int id)=>_ids.ContainsKey(id);
 public bool Take(int id)=>_ids.TryRemove(id,out _);
}
