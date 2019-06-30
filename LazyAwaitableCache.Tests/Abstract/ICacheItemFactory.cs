namespace LazyAwaitableCache.Tests.Abstract
{
    public interface ICacheItemFactory<TCacheItem>
    {
        TCacheItem Create();
    }
}
