using System.Threading.Tasks;

namespace LazyAwaitableCache.Tests.Helper
{
    /// <summary>
    /// Implements extension methods for the Cache class.
    /// Those operations enable the client to build up a Cache
    /// instance with particualr properties.
    /// </summary>
    public static class CacheBuilderExtensions
    {
        /// <summary>
        /// Adds an item to the cache.
        /// </summary>
        /// <typeparam name="TCacheItem">Specifies the type of the cacheitem.</typeparam>
        /// <param name="cache">The cache instance that will be changed.</param>
        /// <param name="key">The key of the item to add.</param>
        /// <param name="item">The item to store under the provided key.</param>
        /// <returns></returns>
        public static Cache<TCacheItem> AddItem<TCacheItem>(this Cache<TCacheItem> cache, string key, TCacheItem item)
        {
            cache.GetStore()[key] = 
                new LazyAwaitableCacheItem<TCacheItem>(() => Task.FromResult(item));
            return cache;
        }
    }
}
