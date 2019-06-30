using LazyAwaitableCache.Abstract;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace LazyAwaitableCache
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCacheItem"></typeparam>
    public class Cache<TCacheItem>
    {
        private ConcurrentDictionary<string, LazyAwaitableCacheItem<TCacheItem>> cache;
        private TimeSpan expiresAfterOnDefault;
        private IAwaitCacheItemStrategy<TCacheItem> awaitingStrategy;

        /// <summary>
        /// Is raised whenever an item is added to the cache.
        /// </summary>
        public event EventHandler<AddItemEventArgs> ItemAdd;

        /// <summary>
        /// Initializes a new instance of the Cache class.
        /// </summary>
        /// <param name="resetOnException">When true, exceptions, raised by the value factories,
        /// are cached and returned on subsequent calls with the same key. Otherwise false. </param>
        public Cache(TimeSpan expiresAfterOnDefault) : 
            this(new AwaitCacheItemSimple<TCacheItem>(), expiresAfterOnDefault) { }

        /// <summary>
        /// Initializes a new instance of the Cache class.
        /// </summary>
        /// <param name="resetOnException">When true, exceptions, raised by the value factories,
        /// are cached and returned on subsequent calls with the same key. Otherwise false. </param>
        public Cache(
            IAwaitCacheItemStrategy<TCacheItem> awaitingStrategy,
            TimeSpan expiresAfterOnDefault)
        {
            this.expiresAfterOnDefault = expiresAfterOnDefault;
            this.cache = new ConcurrentDictionary<string, LazyAwaitableCacheItem<TCacheItem>>();
            this.awaitingStrategy = awaitingStrategy;
            this.ItemAdd += (sender, args) => { Task.Delay(args.Expires).ContinueWith((result) => this.TryRemove(args.Key)); };
        }

        /// <summary>
        /// Loads a particular item out of the cache.
        /// If under the supplied key no item is found,
        /// default(TCacheItem) is retured.
        /// </summary>
        /// <param name="key">Specifies the key of the requested item.
        /// Must not be null, whitespace or empty.</param>
        /// <returns>A Task representing the requested item.</returns>
        public async Task<TCacheItem> GetItem(string key)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            LazyAwaitableCacheItem<TCacheItem> encachedValue = null;
            Task<TCacheItem> cacheItemTask = null;
            if(cache.TryGetValue(key, out encachedValue))
            {
                cacheItemTask = this.awaitingStrategy.AwaitCacheItem(encachedValue);
            }
            else
            {
                cacheItemTask = Task.FromResult<TCacheItem>(default(TCacheItem));
            }
            return await cacheItemTask;
        }

        /// <summary>
        /// Stores an item under a key in the cache.
        /// </summary>
        /// <param name="key">The key for accessing the item.</param>
        /// <param name="itemFactory">A factory operation for creating an item.</param>
        /// <returns>Returning true when the item was stored successfully in the cache.
        ///  Otherwise false.</returns>
        public bool SetItem(
            string key,
            Func<Task<TCacheItem>> itemFactory,
            TimeSpan? expiresAfter = null)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.OnItemAdd(this, new AddItemEventArgs(key, expiresAfter ?? this.expiresAfterOnDefault));

            return this.cache.TryAdd(
                    key,
                    new LazyAwaitableCacheItem<TCacheItem>(itemFactory));
        }

        /// <summary>
        /// Gets or creates an item in the cache.
        /// </summary>
        /// <param name="key">The key under which the item is stored.</param>
        /// <param name="itemFactory">A fsctory operation for creating the item.</param>
        /// <returns>A Task representing the cache item.</returns>
        public async Task<TCacheItem> GetOrCreateItem(
            string key, 
            Func<Task<TCacheItem>> itemFactory,
            TimeSpan? expiresAfter = null)
        { 
            LazyAwaitableCacheItem<TCacheItem> cacheItem = 
                this.cache.GetOrAdd(
                    key, 
                    new LazyAwaitableCacheItem<TCacheItem>(itemFactory));

            this.OnItemAdd(this, new AddItemEventArgs(key, expiresAfter ?? this.expiresAfterOnDefault));

            return await this.awaitingStrategy.AwaitCacheItem(cacheItem);
        }

        /// <summary>
        /// Tries to remove an item from the cache.
        /// </summary>
        /// <param name="key">The of the item.</param>
        /// <param name="removedCacheItem">References the cacheitem when removing succeeded.</param>
        /// <returns>Returns true when removing succeeded. Otherwise false.</returns>
        public bool TryRemove(
            string key, 
            out LazyAwaitableCacheItem<TCacheItem> removedCacheItem)
        {
            return this.cache.TryRemove(key, out removedCacheItem); 
        }

        /// <summary>
        /// Tries to remove an item from the cache.
        /// </summary>
        /// <param name="key">The of the item.</param>
        /// <returns>Returns true when removing succeeded. Otherwise false.</returns>
        public bool TryRemove(string key)
        {
            LazyAwaitableCacheItem<TCacheItem> removedCacheItem;
            return this.cache.TryRemove(key, out removedCacheItem);
        }

        /// <summary>
        /// Gets the concurrent dictionary used as storage engine
        /// for the cache.
        /// </summary>
        /// <returns>The storage of the cache.</returns>
        internal ConcurrentDictionary<string, LazyAwaitableCacheItem<TCacheItem>> GetStore()
        {
            return this.cache;
        }

        /// <summary>
        /// Raises the OnItemAdd event.
        /// </summary>
        /// <param name="sender">Specifies the sender of the event.</param>
        /// <param name="args">Specifies the arguments of the event.</param>
        protected virtual void OnItemAdd(object sender, AddItemEventArgs args)
        {
            if (this.ItemAdd != null)
            {
                Task.Factory.StartNew(() => this.ItemAdd(sender, args));
            }
        }
    }
}
