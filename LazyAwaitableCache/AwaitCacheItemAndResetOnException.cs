using LazyAwaitableCache.Abstract;
using System;
using System.Threading.Tasks;

namespace LazyAwaitableCache
{
    internal class AwaitCacheItemSimpleAndResetOnException<TCacheItem> : IAwaitCacheItemStrategy<TCacheItem>
    {
        public async Task<TCacheItem> AwaitCacheItem(LazyAwaitableCacheItem<TCacheItem> itemToAwait)
        {
            try
            {
                return await itemToAwait.GetTask().ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                itemToAwait.Reset();
                throw ex;
            }
        }
    }
}
