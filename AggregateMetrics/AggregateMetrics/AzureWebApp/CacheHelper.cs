namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using System.Runtime.Caching;

    /// <summary>
    /// Class to contain the one cache for all Gauges 
    /// </summary>
    public class CacheHelper
    {
        /// <summary>
        /// Method saves an object to the cache
        /// </summary>
        /// <param name="cacheKey"> string name of the counter value to be saved to cache</param>
        /// <param name="savedItem"> counter value saved as a cached object</param>
        /// <param name="absoluteExpiration"> DateTimeOffset until item expires from cache</param>
        public static void SaveToCache(string cacheKey, object savedItem, DateTimeOffset absoluteExpiration)
        {
            MemoryCache.Default.Add(cacheKey, savedItem, absoluteExpiration);
        }

        /// <summary>
        /// Retrieves requested item from cache
        /// </summary>
        /// <typeparam name="T"> The desired type of the object to be retrieved from the cache</typeparam>
        /// <param name="cacheKey">Key for the retrieved object</param>
        /// <returns> The requested item, as object type T</returns>
        public static T GetFromCache<T>(string cacheKey) where T : class
        {
            return MemoryCache.Default[cacheKey] as T;
        }

        /// <summary>
        /// Method to check if a key is in a cache
        /// </summary>
        /// <param name="cacheKey">key to search for in cache</param>
        /// <returns>Boolean value for whether or not a key is in the cache</returns>
        public static bool IsInCache(string cacheKey)
        {
            return MemoryCache.Default[cacheKey] != null;
        }
    }
}
