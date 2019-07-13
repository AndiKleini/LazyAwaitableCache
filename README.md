# LazyAwaitableCache
Contains a simple implementation of a lazy .Net Core compatible cache for awaitable cache items. Instead of already 
created/materialized objects, proper async factory operations are put into the cache. Those are awaited by subsequenty reads from the cache. The cache itself is build upon elements in System.Collections.Concurrent (I'm using ConcurrentDictionary), System.Threading.Tasks (the lovely TPL) and Lazy value factory. Putting those powerful capabilities in combination, leads to a threadsafe lock free implementation.

## Encache items
You can encapsulate the creation of an item in the Cache by an asynchronous factory operation. Instead of putting the factory result to the cache, one can simply store corresponding factory.

var yieldItem = await instanceUnderTest.GetOrCreateItem(\r\n
                  "thisIsAKey",
                  () => /*this can be an expensive asynchronous IO operation*/ Task.FromResult(new object()));


