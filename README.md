# LazyAwaitableCache
Contains a simple lock free implementation of a lazy .Net Core compatible cache for awaitable cache items. Instead of already 
created/materialized objects to the cache, proper async factory operations are put into the cache. Those are awaited by accessing cacheitems. The cache itself is build upon elements in System.Collections.Concurrent (I'm using ConcurrentDictionary), System.Threading.Tasks (the lovely TPL) and Lazy value factory.

