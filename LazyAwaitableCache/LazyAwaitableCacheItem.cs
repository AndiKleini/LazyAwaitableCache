using System;
using System.Threading.Tasks;

namespace LazyAwaitableCache
{
    /// <summary>
    /// Implements the LazyAwaitableCacheItem class.
    /// Represents a cache item, whose creation can run asynchronously and
    /// only once in multithreading scenarios.
    /// </summary>
    /// <typeparam name="TCacheItemType"></typeparam>
    public class LazyAwaitableCacheItem<TCacheItemType>
    {
        private Func<Task<TCacheItemType>> factory;
        private Lazy<Task<TCacheItemType>> backingField;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="LazyAwaitableCacheItem"/> class.
        /// </summary>
        /// <param name="factory">A factory method that is used for creating
        /// the item. Must not be null.</param>
        public LazyAwaitableCacheItem(Func<Task<TCacheItemType>> factory)
        {
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

            this.factory = factory;
            this.backingField = new Lazy<Task<TCacheItemType>>(factory);
        }

        /// <summary>
        /// Gets a task representing the cacheitem's value.
        /// </summary>
        /// <returns>A task whose value represents the value of the cacheitem.</returns>
        public Task<TCacheItemType> GetTask()
        {
            return this.backingField.Value;
        }

        /// <summary>
        /// Resets the value of the cacheitem.
        /// The next time when it will be accessed, the factory 
        /// will be executed again.
        /// </summary>
        public void Reset()
        {
            this.backingField = new Lazy<Task<TCacheItemType>>(this.factory);
        }
    }
}
