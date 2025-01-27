using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using BonfireServer.Internal.Context;
using BonfireServer.Internal.Paths;

namespace BonfireServer.Internal.Event;

public abstract class BaseEvent
{
    private static readonly Dictionary<Type, Dictionary<LiteFlakeId, WebSocket>> EventTargets = new();
    
    public static Dictionary<LiteFlakeId, WebSocket> GetTargets<T>() where T : BaseEvent
    {
        var type = typeof(T);
        if (!EventTargets.TryGetValue(type, out var targets))
        {
            targets = new Dictionary<LiteFlakeId, WebSocket>();
            EventTargets[type] = targets;
        }
        return targets;
    }

    public static void AddTarget(Type type, LiteFlakeId id, WebSocket target)
    {
        if (!EventTargets.TryGetValue(type, out var targets))
            EventTargets.Add(type, new Dictionary<LiteFlakeId, WebSocket>());

        EventTargets[type][id] = target;
    } 
    public static void RemoveTarget(Type type, LiteFlakeId id)
    {
        if (EventTargets.TryGetValue(type, out var targets))
            targets.Remove(id);
    }  
    public static void RemoveTargetFromAll(LiteFlakeId id)
    {
        foreach (var targets in EventTargets.Values)
        {
            targets.Remove(id);
        }
    }
    
    protected abstract Dictionary<LiteFlakeId, WebSocket> Targets { get; }

    public virtual void Emit<T>(T ctx) where T : IBaseContext
    {
        var evt = new
        {
            EventType = typeof(T).Name,
            Payload = ctx
        };
    
        var json = JsonSerializer.Serialize(evt);
        var data = Encoding.UTF8.GetBytes(json);
    
        foreach (var target in Targets)
        {
            WebsocketServer.SendData(target.Key, target.Value, data);
        }
    }
}