using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace LazyAwaitableCache.IntegrationTests
{
    /// <summary>
    /// Tests the common scenario of putting an instance to the 
    /// cache and afterwards reading the same from the cache.
    /// </summary>
    [TestFixture]
    public class StoreAndReadFromCache_SingleThread
    {
        /// <summary>
        /// Executes the integation test.
        /// </summary>
        [Test]
        public async Task GetOrCreateItem_CacheDoesNotContainItem_PerformsFlawless()
        {
            Cache<string> cache = new Cache<string>(
                TimeSpan.FromSeconds(1));
            string key = "key";
            string valueToPutInCache = "ValueOfKey1";

            string encachedInitially = await cache.GetOrCreateItem(
                   key,
                   () => Task.FromResult(valueToPutInCache));

            string encachedSubsequently = await cache.GetItem(key);

            Assert.AreEqual(encachedInitially, encachedSubsequently);
            Assert.AreEqual(encachedSubsequently, valueToPutInCache);
        }

        /// <summary>
        /// Sets a particualr item into to the cache.
        /// It is expected that the item is emitted on the next call.
        /// </summary>
        [Test]
        public async Task SetItem_ItemIsStoredInCache_ItemIsEmittedOnNextRequest()
        {
            Cache<string> cache = new Cache<string>(
                TimeSpan.FromSeconds(1));
            string key = "key";
            string valueToPutInCache = "ValueOfKey1";

           cache.SetItem(
                   key,
                   () => Task.FromResult(valueToPutInCache));

            string encachedSubsequently = await cache.GetItem(key);

            Assert.AreEqual(encachedSubsequently, valueToPutInCache);
        }

        /// <summary>
        /// Tests the case where the requested item was already expired.
        /// The cache is expected to return a default instance.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GetItem_ItemAlreadyExpired_ReceiveDefaultInstance()
        {
            Cache<string> cache = new Cache<string>(
                TimeSpan.FromSeconds(1));
            string key = "key";
            string valueToPutInCache = "ValueOfKey1";

            string encachedInitially = await cache.GetOrCreateItem(
                   key,
                   () => Task.FromResult(valueToPutInCache));

            await Task.Delay(2000);

            string encachedAfterExpired = await cache.GetItem(key);

            Assert.IsNull(encachedAfterExpired);
        }

        /// <summary>
        /// Tests the tryRemove operation when an instance to remove is in the cache.
        /// It is expected that true is returned and the item is not returned by the 
        /// cache anylonger.
        /// </summary>
        [Test]
        public async Task TryRemoveSimple_ItemInCache_ReturnsTrue()
        {
            Cache<string> cache = new Cache<string>(
                TimeSpan.FromSeconds(10));
            string key = "key";
            string valueToPutInCache = "ValueOfKey1";

            string encachedInitially = await cache.GetOrCreateItem(
                   key,
                   () => Task.FromResult(valueToPutInCache));

            bool tryRemoveResult = cache.TryRemove(key);

            Assert.IsTrue(tryRemoveResult);
            Assert.IsNull(await cache.GetItem(key));
        }

        /// <summary>
        /// Tests the tryRemove operation when an instance to remove is in the cache.
        /// It is expected that true is returned and the item is not returned by the 
        /// cache anylonger.
        /// </summary>
        [Test]
        public async Task TryRemoveAdvanced_ItemInCache_ReturnsTrue()
        {
            Cache<string> cache = new Cache<string>(
                TimeSpan.FromSeconds(10));
            string key = "key";
            string valueToPutInCache = "ValueOfKey1";
            LazyAwaitableCacheItem<string> removedInstance = null;

            string encachedInitially = await cache.GetOrCreateItem(
                   key,
                   () => Task.FromResult(valueToPutInCache));

            bool tryRemoveResult = cache.TryRemove(key, out removedInstance);

            Assert.IsTrue(tryRemoveResult);
            Assert.IsNull(await cache.GetItem(key));
            Assert.AreEqual(encachedInitially, await removedInstance.GetTask());
        }
    }
}
