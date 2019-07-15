# LazyAwaitableCache
Contains a simple implementation of a lazy .Net Core compatible cache for awaitable cache items. Instead of already 
created/materialized objects, proper async factory operations are put into the cache. Those are awaited by subsequenty reads from the cache. The cache itself is build upon elements in System.Collections.Concurrent (I am using ConcurrentDictionary), System.Threading.Tasks (the lovely TPL) and Lazy value factory. Putting those powerful capabilities in combination, leads to a threadsafe lock free implementation.

## Create the cache instance
Instances of the cache are created via constructor. The example below creates an instance whose items will per default reside for 5 seconds within the cache. 
```C#
var defaultExpirationOf5Seconds = TimeSpan.FromMilliseconds(5000);
var myCacheInstance = new Cache<string>(defaultExpirationOf5Seconds)
```

## Encache items
You can encapsulate the creation of an item in the Cache by an asynchronous factory operation. Instead of putting the factory result to the cache, one can simply store corresponding factory method. The factory evaluates when a client requests/awaits the item from the cache. The operation is threadsafe, so that even when accessed concurrent by different threads, factory operation is only called once.

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

If you want to set an item straight forward into the cache you can use GetItem operation.

```C#
bool setItemResult = await myCacheInstance.SetItem(
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

## Deal with exceptions in factories
It could always somehow happen that you factory operations raise an exception when those are awaited. The cache can handle this in several ways.
### Rethrow and cache exception produced by factory
Aligned to Lazy value factory pattern a raised exception can be cached by and delivered to each subsequently requesting client. If you want to cache the exception as result form the factory until the cache item expires, you can use enum value AwaitCacheItemStrategyType.AwaitAndCachEachFactoryResult at construction.
```C#
Cache<string> cache = new Cache<string>(
                AwaitCacheItemStrategyType.AwaitAndCachEachFactoryResult,
                TimeSpan.FromSeconds(10));
```
### Rethrow but don't cache exception produced by factory
In some cases you don't want to cache yield exceptions. Instead it could make more sense to retry factory method and caching its result only in case of flawless execution. For this purpose you can create cache instance per default or passing enum value AwaitCacheItemStrategyType.AwaitAndCacheOnlyOnFlawlessExecution to constructor.
```C#
Cache<string> cache = new Cache<string>(
                AwaitCacheItemStrategyType.AwaitAndCacheOnlyOnFlawlessExecution,
                TimeSpan.FromSeconds(10));
```

## Roadmap
Planned extensions are:
1. Create a nuget package.
2. Supporting different strategies for item expiration. By know for each item a delayed task is launched, whose expiration triggers removement of item.

Let me know what could be helpful for you and I will give my best to adapt roadmap properly.

## Contribution
If you have an ideas or needs for changed, don't hesitate to contact me. I will also handle you pull requests.

## Contact
If you need anything (questions, ideas, ...) please contact me under Andi.Kleinbichler@gmail.com.


