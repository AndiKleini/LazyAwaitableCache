using System.Threading.Tasks;

namespace LazyAwaitableCache.Abstract
{
    internal interface IAwaitCacheItemStrategy<TCacheItem>
    {
        Task<TCacheItem> AwaitCacheItem(LazyAwaitableCacheItem<TCacheItem> itemToAwait);
    }
}
