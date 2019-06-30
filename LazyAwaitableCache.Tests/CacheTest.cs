using NUnit.Framework;
using System;
using System.Threading.Tasks;
using LazyAwaitableCache.Tests.Helper;
using LazyAwaitableCache.Tests.Abstract;
using Moq;
using System.Threading;

namespace LazyAwaitableCache.Tests
{
    /// <summary>
    /// Comprises the tests for the Cache class.
    /// </summary>
    [TestFixture]
    public class CacheTest
    {
        /// <summary>
        /// Tests the access of an existing item in the cache.
        /// It is expected that the item is returned.
        /// </summary>
        /// <returns>Task for running the operation asynchronously.</returns>
        [Test]
        public async Task GetItem_ItemIsInCache_ReturnsItem()
        {
            string key = "mykey";
            object item = new object();
            var instanceUnderTest = 
                this.CreateInstanceUnderTest<object>()
                .AddItem(key, item);
        
            var yieldItem = await instanceUnderTest.GetItem(key);

            Assert.AreSame(yieldItem, item);
        }

        /// <summary>
        /// Tests the acces of a non existing item in the cache.
        /// It is expected that the cache returns null.
        /// </summary>
        /// <returns>Task for running opersation asnychronously.</returns>
        [Test]
        public async Task GetItem_ItemIsNotInCache_ReturnsNull()
        {
            var instanceUnderTest = this.CreateInstanceUnderTest<object>();

            var yieldItem = await instanceUnderTest.GetItem("anyUnknownKey");

            Assert.IsNull(yieldItem);
        }

        /// <summary>
        /// Tests the faulting behaviour when the supplied key is invalid.
        /// </summary>
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void GetItem_SuppliedKeyIsInvalid_Throws(string key)
        {
            var instanceUnderTest = this.CreateInstanceUnderTest<object>();

            var exception = Assert.ThrowsAsync<ArgumentNullException>(
                    async () => await instanceUnderTest.GetItem(key));
        }

        /// <summary>
        /// Tests the faulting behaviour when the supplied key is invalid.
        /// </summary>
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void SetItem_SuppliedKeyIsInvalid_Throws(string key)
        {
            var instanceUnderTest = this.CreateInstanceUnderTest<object>();

            var exception = Assert.Throws<ArgumentNullException>(
                    () => instanceUnderTest.SetItem(key, () => null));
        }

        /// <summary>
        /// Tests the faulting behaviour when the supplied factory delegat is invalid.
        /// </summary>
        [Test]
        public void SetItem_SuppliedFactoryIsNull_Throws()
        {
            var instanceUnderTest = this.CreateInstanceUnderTest<object>();

            var exception = Assert.Throws<ArgumentNullException>(
                    () => instanceUnderTest.SetItem("anykey", null));
        }

        /// <summary>
        /// Tests adding an item to the cache.
        /// Only expectation is that the method does not throw.
        /// </summary>
        [Test]
        public void SetItem_ValidKeyAndItem_PerformsFlawless()
        {
            var instanceUnderTest = this.CreateInstanceUnderTest<object>();

            Assert.DoesNotThrow(() => 
                instanceUnderTest.SetItem(
                    "anykey", 
                     () => Task.FromResult(new object())));
        }

        /// <summary>
        /// Tests the case when an item is created the first time.
        /// </summary>
        /// <returns>A task to await the operation.</returns>
        [Test]
        public async Task GetOrCreateItem_ItemIsNotInCache_IsCreated()
        {
            var instanceUnderTest = this.CreateInstanceUnderTest<object>();
            var item = new object();

            var yieldItem = await instanceUnderTest.GetOrCreateItem(
                "anykey",
                () => Task.FromResult(item));

            Assert.AreSame(item, yieldItem);
        }

        /// <summary>
        /// Tests the case when an item is created the first time.
        /// </summary>
        /// <returns>A task to await the operation.</returns>
        [Test]
        public async Task GetOrCreateItem_ItemIsAlreadyInCache_IsCreated()
        {
            string key = "mykey";
            var item = new object();
            var instanceUnderTest = 
                this.CreateInstanceUnderTest<object>()
                .AddItem<object>(key, item);
            var handleAddItemMock = new Mock<IHandleItemAdd>();
            TaskCompletionSource<bool> addItemWasFired = new TaskCompletionSource<bool>();
            instanceUnderTest.ItemAdd += async (sender, args) =>
            {
                await handleAddItemMock.Object.OnItemAdd(sender, args);
                addItemWasFired.SetResult(true);
            };
               
            var yieldItem = await instanceUnderTest.GetOrCreateItem(
                key,
                () => Task.FromResult(new object()));

            Assert.Multiple(async () =>
            {
                Assert.AreSame(item, yieldItem);
                await addItemWasFired.Task;
                handleAddItemMock.Verify(
                    m => m.OnItemAdd(
                        It.Is<Cache<object>>(t => ReferenceEquals(t, instanceUnderTest)),
                        It.IsAny<AddItemEventArgs>()), 
                    Times.Once());
            });  
        }

        /// <summary>
        /// Tests the removing of an existing item.
        /// The item should be removed and returned as out put parameter.
        /// </summary>
        [Test]
        public async Task TryRemove_ItemInCache_ReturnsItemAsOutParameterAndEmitsTrue()
        {
            string key = "mykey";
            object item = new object();
            var instanceUnderTest =
                this.CreateInstanceUnderTest<object>()
                .AddItem(key, item);
            LazyAwaitableCacheItem<object> yieldOutParameter = null;

            bool removeSucceeded = instanceUnderTest.TryRemove(key, out yieldOutParameter);
            object awaitedCacheItem = await yieldOutParameter.GetTask();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(removeSucceeded);
                Assert.AreSame(item, awaitedCacheItem);
            });
        }

        /// <summary>
        /// Tests the removing of a not existing item.
        /// The operation returns false and the oit parameter references the item.
        /// </summary>
        [Test]
        public void TryRemove_ItemNotInCache_ReturnsNullAsOutParameterAndEmitsFalse()
        {
            string key = "mykey";
            object item = new object();
            var instanceUnderTest =
                this.CreateInstanceUnderTest<object>();
            LazyAwaitableCacheItem<object> yieldOutParameter = null;

            bool removeSucceeded = instanceUnderTest.TryRemove(key, out yieldOutParameter);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(removeSucceeded);
                Assert.IsNull(yieldOutParameter);
            });
        }

        /// <summary>
        /// Tests the removing of an existing item.
        /// The item should be removed and operation returns true.
        /// </summary>
        [Test]
        public void TryRemove_ItemInCache_ReturnsTrue()
        {
            string key = "mykey";
            object item = new object();
            var instanceUnderTest =
                this.CreateInstanceUnderTest<object>()
                .AddItem(key, item);

            bool removeSucceeded = instanceUnderTest.TryRemove(key);

            Assert.IsTrue(removeSucceeded);
        }

        /// <summary>
        /// Tests the removing of a not existing item.
        /// The operation returns false.
        /// </summary>
        [Test]
        public void TryRemove_ItemNotInCache_ReturnsFalse()
        {
            string key = "mykey";
            object item = new object();
            var instanceUnderTest =
                this.CreateInstanceUnderTest<object>();

            bool removeSucceeded = instanceUnderTest.TryRemove(key);

            Assert.IsFalse(removeSucceeded);
        }

        /// <summary>
        /// Craetes a cache instance one can act on during testing.
        /// </summary>
        /// <typeparam name="TCacheItem">Type argument of the cache.</typeparam>
        /// <returns>The created cache instance.</returns>
        private Cache<TCacheItem> CreateInstanceUnderTest<TCacheItem>()
        {
            return new Cache<TCacheItem>(TimeSpan.FromMilliseconds(5000));
        }
    }
}
