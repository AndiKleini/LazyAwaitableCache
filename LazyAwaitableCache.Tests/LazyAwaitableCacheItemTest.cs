using LazyAwaitableCache.Tests.Abstract;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using Moq;

namespace LazyAwaitableCache.Tests
{
    /// <summary>
    /// Contains the tests for the LazyAwaitableCacheItem class.
    /// </summary>
    [TestFixture]
    public class LazyAwaitableCacheItemTest
    {
        /// <summary>
        /// Tests the result of the task after the CacheItem was created
        /// with a proper factory.
        /// It is expected to await the result of the factory method.
        /// </summary>
        [Test]
        public async Task AwaitTaskOfCacheItem_TaskEmitsValueOfFactory()
        {
            object created = new object();
            var cacheItem = 
                new LazyAwaitableCacheItem<object>(() => Task.FromResult(created));

            var result = await cacheItem.GetTask();
            Assert.AreSame(created, result);
        }

        /// <summary>
        /// Tests the result of the task after the CacheItem was created
        /// with a proper factory.
        /// It is expected to await the result of the factory method.
        /// </summary>
        [Test]
        public void SynchronouslyEvaluateTaskOfCacheItem_TaskEmitsValueOfFactory()
        {
            object created = new object();
            var cacheItem =
                new LazyAwaitableCacheItem<object>(() => Task.FromResult(created));

            var result = cacheItem.GetTask().Result;
            Assert.AreSame(created, result);
        }

        /// <summary>
        /// Tests the propagation of an exception, raised in factory, by awaiting 
        /// the task.
        /// </summary>
        [Test]
        public void AwaitTaskOfCacheItem_TaskThrowsExcpetionRaisedByFactory()
        {
            var exception = new Exception();
            var cacheItem =
                new LazyAwaitableCacheItem<object>(() => throw exception);

            var yieldException = Assert.ThrowsAsync<Exception>(async () => await cacheItem.GetTask());
            Assert.AreSame(exception, yieldException);
        }

        /// <summary>
        /// Tests the propagation of an exception, raised in factory, by awaiting 
        /// the task.
        /// </summary>
        [Test]
        public void SynchronouslyEvaluateTaskOfCacheItem_TaskThrowsExcpetionRaisedByFactory()
        {
            var exception = new Exception();
            var cacheItem =
                new LazyAwaitableCacheItem<object>(() => throw exception);

            var yieldException = Assert.Throws<Exception>(() => { var tmp = cacheItem.GetTask().Result; });
            Assert.AreSame(exception, yieldException);
        }

        /// <summary>
        /// Tests the reset behaviour of the class.
        /// Whenever Reset operation is invoked on the instance,
        /// the cached value of the factory has to be cleared and
        /// on the next request the factory is invoked again.
        /// </summary>
        [Test]
        public async Task ResetLazyCahceItemFactory_FactoryIsInvokedAgain()
        {
            object createdFirst = new object(),
                   createdSecond = new object();
            var mock = new Mock<ICacheItemFactory<object>>();
            mock.SetupSequence(m => m.Create())
                .Returns(createdFirst)
                .Returns(createdSecond);
            Func<Task<object>> mockedFactory = () => Task.FromResult(mock.Object.Create());
            var cacheItem = new LazyAwaitableCacheItem<object>(() => mockedFactory());

            var resultYieldFirst = await cacheItem.GetTask();
            cacheItem.Reset();
            var resultYieldSecond = await cacheItem.GetTask();

            Assert.Multiple(() => {
                Assert.AreSame(createdFirst, resultYieldFirst);
                Assert.AreSame(createdSecond, resultYieldSecond);
            });
        }
    }
}
