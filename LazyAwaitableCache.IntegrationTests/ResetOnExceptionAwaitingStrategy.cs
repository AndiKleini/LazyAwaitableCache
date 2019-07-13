using LazyAwaitableCache.IntegrationTests.Abstract;
using NUnit.Framework;
using System;
using Moq;
using System.Threading.Tasks;

namespace LazyAwaitableCache.IntegrationTests
{
    /// <summary>
    /// Contains the integration tests for the ResestOnExceptionAwaitingStrategy class.
    /// </summary>
    [TestFixture]
    public class ResetOnExceptionAwaitingStrategy
    {
        /// <summary>
        /// Tests the case when a cacheitem is set into the cache and its
        /// factory operation throws. When AwaitCacheItemStrategy Reset on exception is choosen,
        /// the client has to receive the exception by awaiting the cacheitem and the cacheItem
        /// Value factory is reseted.
        /// </summary>
        [Test]
        public async Task GetItem_FactoryThrows_ExceptionPropagatedAndItemReset()
        {
            Cache<string> cache = new Cache<string>(
                AwaitCacheItemStrategyType.AwaitAndCacheOnlyOnFlawlessExecution,
                TimeSpan.FromSeconds(10));
            string key = "key";
            Exception firstException = new Exception(),
                      secondException = new Exception();

            var factoryMock = new Mock<ICacheItemFactory<string>>();
            factoryMock.SetupSequence(m => m.Create())
                .Throws(firstException)
                .Throws(secondException);

            cache.SetItem(key, () =>
            {
                factoryMock.Object.Create();
                return Task.FromResult("TestString");
            });

            var firstReceivedException = Assert.ThrowsAsync<Exception>(async () => await cache.GetItem(key));
            Assert.AreEqual(firstException, firstReceivedException);
            var secondReceivedException = Assert.ThrowsAsync<Exception>(async () => await cache.GetItem(key));
            Assert.AreEqual(secondException, secondReceivedException);
        }

        /// <summary>
        /// Tests the case when a cacheitem is set into the cache and its
        /// factory operation throws. When AwaitCacheItemStrategy AwaitCacheItemSimple is choosen (per deafualt),
        /// the client has to receive the exception by awaiting the cacheitem whenever the item is retrieved.
        /// </summary>
        [Test]
        public async Task GetItem_FactoryThrows_ExceptionCachedAndPropagated()
        {
            Cache<string> cache = new Cache<string>(
               TimeSpan.FromSeconds(10));
            string key = "key";
            Exception firstException = new Exception(),
                      secondException = new Exception();
            
            var factoryMock = new Mock<ICacheItemFactory<string>>();
            factoryMock.SetupSequence(m => m.Create())
                .Throws(firstException)
                .Throws(secondException);

            cache.SetItem(key, () =>
            {
                factoryMock.Object.Create();
                return Task.FromResult("TestString");
            });

            var firstReceivedException = Assert.ThrowsAsync<Exception>(async () => await cache.GetItem(key));
            Assert.AreEqual(firstException, firstReceivedException);
            var secondReceivedException = Assert.ThrowsAsync<Exception>(async () => await cache.GetItem(key));
            Assert.AreEqual(firstException, secondReceivedException);
        }
    }
}
