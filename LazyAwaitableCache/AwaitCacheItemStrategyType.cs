namespace LazyAwaitableCache
{
    /// <summary>
    /// Specifies the behaviour by awaiting cache items. 
    /// </summary>
    public enum AwaitCacheItemStrategyType
    {
        /// <summary>
        /// Indicates the the cache items are simply awaited independent
        /// of factory result. Even if factory raises an exception. 
        /// The cache will cache and deliver every result produced by the factory.
        /// </summary>
        AwaitAndCachEachFactoryResult = 0,

        /// <summary>
        /// Indicates that the factory result is cache if and only if the factory 
        /// performs flawless. In other words the factory execution does not emit
        /// an exception.
        /// </summary>
        AwaitAndCacheOnlyOnFlawlessExecution = 1
    }
}
