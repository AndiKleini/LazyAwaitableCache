using System.Threading.Tasks;

namespace LazyAwaitableCache.Abstract
{
    public interface IAwaitCacheItemStrategy<TCacheItem>
    {
        Task<TCacheItem> AwaitCacheItem(LazyAwaitableCacheItem<TCacheItem> itemToAwait);
    }
}
