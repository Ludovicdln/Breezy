using System.Reflection.Metadata;

namespace Breezy.Benchmarks.Extensions;

public sealed class MemoryCacheableQuery<T> : ICacheableQuery<T> where T : class
{
    private readonly Dictionary<IdentityQuery, Tuple<DateTime, IEnumerable<T>>> _cacheableData = new();
    
    public Task<IEnumerable<T>> GetCacheableResultsAsync(IdentityQuery identityQuery)
    {
        if (_cacheableData.TryGetValue(identityQuery, out var results))
        {
            var (addDate, collection) = results;

            if ((DateTime.Now - addDate) < TimeSpan.FromSeconds(10))
                return Task.FromResult<IEnumerable<T>>(collection);

            _cacheableData.Remove(identityQuery);
        }

        return Task.FromResult<IEnumerable<T>>(Array.Empty<T>());
    }

    public Task SetCacheableResultsAsync(IdentityQuery identityQuery, IEnumerable<T> results)
    {
        _cacheableData.Add(identityQuery, new Tuple<DateTime, IEnumerable<T>>(DateTime.Now, results));

        return Task.CompletedTask;
    }
}