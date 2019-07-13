# LazyAwaitableCache
Contains a simple implementation of a lazy .Net Core compatible cache for awaitable cache items. Instead of already 
created/materialized objects, proper async factory operations are put into the cache. Those are awaited by subsequenty reads from the cache. The cache itself is build upon elements in System.Collections.Concurrent (I'm using ConcurrentDictionary), System.Threading.Tasks (the lovely TPL) and Lazy value factory. Putting those powerful capabilities in combination, leads to a threadsafe lock free implementation.

## Create the cache instance
Instances of the cache are created via constructor. The example below creates an instance whose items will per default reside for 5 seconds within the cache. 
```C#
var defaultExpirationOf5Seconds = TimeSpan.FromMilliseconds(5000);
var myCacheInstance = new Cache<string>(defaultExpirationOf5Seconds)
```

## Encache items
You can encapsulate the creation of an item in the Cache by an asynchronous factory operation. Instead of putting the factory result to the cache, one can simply store corresponding factory method. The factory will be evaluated when a client requests/awaits the item from the cache. The operation is threadsafe, so that even when accessed concurrent by different threads, factory operation is only called once.

The following example reads the data for the string from a file, which is an IO operation typically implemented in asynchronous fashion. With GetOrCreate one needs not to take particualt care in concurrent scenarios. When multiple threads try to put an item under the same key to the dictionary, only one will make the race. All other clients will get the result of the winning thread.

```C#
var yieldItem = await myCacheInstance.GetOrCreateItem(
                "thisIsAKey",
                async () => await File.ReadAllTextAsync("C:\thisIsaFile.txt"));
```

It is also possible to overwrite the default expiration time of a cacheitem with encache operation.

```C#
var yieldItem = await myCacheInstance.GetOrCreateItem(
                "thisIsAKey",
                async () => await File.ReadAllTextAsync("C:\thisIsaFile.txt"),
                TimeSpan.FromSeconds(4));
```

## Read from cache
Passing an items key to operation GetItem, emits the item from the cache.
```C#
string encachedSubsequently = await myCacheInstance.GetItem("thisIsAKey");
```

## Remove item from cache
For removing items from cache, a TryRemove operation is in place. It emits a flag indicating whether the item was removed or not. If and ony if returning true, a reference to the item can be fetched by an out parameter. In the latter case, the item already expired and the reference of the item points to the default instance of the cacheitem value (e.g. null, 0, ...).
```C#
LazyAwaitableCacheItem<string> removedInstance = null;
bool tryRemoveResult = cache.TryRemove("thisIsAKey", out removedInstance);
```
If you don't need reference to the removed item, you can simply use a proper overload without the output parameter.
```C#
bool tryRemoveResult = cache.TryRemove("thisIsAKey");
```
