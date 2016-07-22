namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using System.Net.Http;
    using System.Runtime.Caching;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to contain the one cache for all Gauges 
    /// </summary>
    internal class CacheHelper
    {
        /// <summary>
        /// Checks if a key is in the cache and if not
        /// Retrieves raw counter data from Environment Variables
        /// Cleans raw JSON for only requested counter
        /// Creates value for caching
        /// </summary>
        /// <param name="name">cache key and name of the counter to be selected from JSON</param>
        /// <returns>value from cache</returns>
        public static int GetCountervalue(string name)
        {
            const string jsonKey = "json";
            if (!CacheHelper.IsInCache(jsonKey))
            {
               
                PerformanceCounterImplementation client = new PerformanceCounterImplementation();
                string uncachedJson= client.GetAzureWebAppEnvironmentVariables(AzureWebApEnvironmentVariables.All);

                /* http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ASPNET/
                http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/
                http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_CLR/
               http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ALL/ */

                if (uncachedJson == null)
                {
                    return 0;
                }

                CacheHelper.SaveToCache(jsonKey, uncachedJson, DateTimeOffset.Now.AddSeconds(5.0));
            }

            string json = GetFromCache(jsonKey).ToString();
            string jsonSubstring = json.Substring(json.IndexOf(name), json.Length - json.IndexOf(name));

            int startingIdx = jsonSubstring.IndexOf(" ");
            int endingIdx = jsonSubstring.IndexOf(",");

            string value = jsonSubstring.Substring(startingIdx + 1, endingIdx - startingIdx - 1);

            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Method saves an object to the cache
        /// </summary>
        /// <param name="cacheKey"> string name of the counter value to be saved to cache</param>
        /// /<param name="toCache">Object to be cached</param>
        /// <param name="absoluteExpiration"> DateTimeOffset until item expires from cache</param>
        public static void SaveToCache(string cacheKey, object toCache,  DateTimeOffset absoluteExpiration)
        {  
            MemoryCache.Default.Add(cacheKey, toCache, absoluteExpiration);
        }

        /// <summary>
        /// Retrieves requested item from cache
        /// </summary>
        /// <param name="cacheKey">Key for the retrieved object</param>
        /// <returns> The requested item, as object type T</returns>
        public static object GetFromCache(string cacheKey) 
        {
            return MemoryCache.Default[cacheKey];
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
