using System.Collections.Concurrent;
using BonfireServer.Internal;

namespace BonfireServer.Database;

public interface ICachableType
{
    public LiteFlakeId Id { get; }
}

public class DatabaseCache<T> where T : ICachableType
{
    private readonly ConcurrentDictionary<long, T> _cache = new();

    public bool Add(T item)
    {
        var success = _cache.TryAdd(item.Id.Val, item);
        if (!success)
            Logger.Warn($"Tried saving duplicate ID: {item.Id.Val}");

        return success;
    }
    public bool Get(LiteFlakeId id, out T? obj) => Get(id.Val, out obj);
    public bool Get(long id, out T? obj)
    {
        return _cache.TryGetValue(id, out obj);
    }
    
    public bool Remove(LiteFlakeId id, out T? obj) => Remove(id.Val, out obj);
    public bool Remove(long id, out T? obj)
    {
        return _cache.TryRemove(id, out obj);
    }

    public void Clear() => _cache.Clear();
}