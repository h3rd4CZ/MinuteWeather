using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiNotifications.Services
{
    public interface IAsynchronousCache<TValue> where TValue : class
    {
        Task<IList<KeyValuePair<CacheKey, TValue>>> GetCacheContentAsync();
        Task<TValue> GetOrFetchValueAsync(CacheKey key, Func<Task<TValue>> valueProvider);
        Task InvalidateCacheAsync();
        Task InvalidateCacheItemAsync(CacheKey key);
    }
}
