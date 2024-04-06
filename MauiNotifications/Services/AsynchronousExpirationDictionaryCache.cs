using Microsoft.Extensions.Logging;

namespace MauiNotifications.Services
{
    public class AsynchronousExpirationDictionaryCache<TValue> : IAsynchronousCache<TValue> where TValue : class
    {
        private const int EXP_TIMEINMINUTES = 60;

        private readonly ILogger<AsynchronousExpirationDictionaryCache<TValue>> traceLogger;
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public IDictionary<CacheKey, ExpirationDictionaryCacheItem<TValue>> _cache { get; }
            = new Dictionary<CacheKey, ExpirationDictionaryCacheItem<TValue>>();

        public AsynchronousExpirationDictionaryCache(ILogger<AsynchronousExpirationDictionaryCache<TValue>> traceLogger)
        {
            this.traceLogger = traceLogger;
        }


        public async Task<IList<KeyValuePair<CacheKey, TValue>>> GetCacheContentAsync() =>
                await UseLockAndReturn(async () =>
                        await Task.FromResult(_cache.ToList().Select(c => new KeyValuePair<CacheKey, TValue>(c.Key, c.Value.Value))
                            .ToList())
                );

        public async Task<TValue> GetOrFetchValueAsync(CacheKey key, Func<Task<TValue>> valueProvider)
        {
            if (key == null)
                throw new ArgumentNullException("key");

            var expirationDuration = EXP_TIMEINMINUTES;

            return await UseLockAndReturn(async () =>
            {
                _cache.TryGetValue(key, out ExpirationDictionaryCacheItem<TValue> value);

                if (Equals(null, value)) return await FetchValue(key, valueProvider, Equals(null, value));

                var now = DateTime.Now;

                if (value.InsertionDateTime.AddMinutes(expirationDuration) < now)
                {
                    try
                    {
                        return await FetchValue(key, valueProvider, true);
                    }
                    catch { return value.Value; }
                }
                else
                {
                    return value.Value;
                }
            });
        }

        private async Task<TValue> FetchValue(CacheKey key, Func<Task<TValue>> valueProvider, bool refreshItem)
        {
            traceLogger.LogTrace("Cache of {0}: expired for key '{1}', fetching value", typeof(TValue), key);
                        
            TValue value = await valueProvider();

            if (value != null && refreshItem)
            {
                await Task.Run(() =>
                {
                    var now = DateTime.Now;

                    _cache[key] = ExpirationDictionaryCacheItem<TValue>.Create(value, now);
                });
            }
            else
                traceLogger.LogTrace("Cache of {0}: null value fetched", typeof(TValue));

            return value;
        }

        public async Task ClearAsync()
        {
            await UseLock(async () =>
            {
                await Task.Run(() =>
                {
                    traceLogger.LogTrace("Cache of {0}: clearing requested", typeof(TValue));
                    _cache.Clear();
                });
            });
        }

        public async Task InvalidateCacheItemAsync(CacheKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            await UseLock(async () =>
            {
                await Task.Run(() => _cache.Remove(key));
            });
        }

        public async Task InvalidateCacheAsync()
        {
            await UseLock(async () =>
            {
                await Task.Run(() => _cache.Clear());
            });
        }

        private async Task UseLock(Func<Task> task)
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                await task();
            }
            finally { semaphoreSlim.Release(); }
        }

        private async Task<TReturn> UseLockAndReturn<TReturn>(Func<Task<TReturn>> task)
        {
            await semaphoreSlim.WaitAsync();

            try
            {
                return await task();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
