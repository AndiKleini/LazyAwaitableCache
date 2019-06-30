using LazyAwaitableCache.Abstract;
using System;
using System.Threading.Tasks;

namespace LazyAwaitableCache
{
    internal class AwaitCacheItemSimple<TCacheItem> : IAwaitCacheItemStrategy<TCacheItem>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemToAwait"></param>
        /// <returns></returns>
        public async Task<TCacheItem> AwaitCacheItem(LazyAwaitableCacheItem<TCacheItem> itemToAwait)
        {
            return await itemToAwait.GetTask().ConfigureAwait(false);
        }
    }
}
