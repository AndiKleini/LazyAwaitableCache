namespace LazyAwaitableCache.IntegrationTests.Abstract
{
    public interface ICacheItemFactory<TCacheItem>
    {
        TCacheItem Create();
    }
}
