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
        /// <param name="name">cache key and name of the counter to be selected from json</param>
        /// <returns>value from cache</returns>
        public static int GetCountervalue(string name)
        {
            if (!CacheHelper.IsInCache(name))
            {
                HttpClient client = new HttpClient();

                /* http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ASPNET/
                http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/
                http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_CLR/
               http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ALL/ */
                Task<string> counterRetrieval = client.GetStringAsync("http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/");
                counterRetrieval.Wait();

                string uncachedJson = counterRetrieval.Result;

                CacheHelper.SaveToCache(name, uncachedJson, DateTimeOffset.Now.AddSeconds(5.0));
            }

            string json = GetFromCache(name).ToString();
            string prefix = "\\\"" + name + "\\\":";
            string postfix = ",\\";

            int idx = json.IndexOf(prefix) + prefix.Length;
            int endIdx = json.IndexOf(postfix, idx);

            string value = json.Substring(idx, endIdx - idx);
            
            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Method saves an object to the cache
        /// </summary>
        /// <param name="cacheKey"> string name of the counter value to be saved to cache</param>
        /// /<param name="toCache">Object to be cached</param>
        /// <param name="absoluteExpiration"> DateTimeOffset until item expires from cache</param>
        public static void SaveToCache(string cacheKey,object toCache, DateTimeOffset absoluteExpiration)
        {
           
            MemoryCache.Default.Add(cacheKey, toCache, absoluteExpiration);
        }

        /// <summary>
        /// Retrieves requested item from cache
        /// </summary>
        /// <typeparam name="T"> The desired type of the object to be retrieved from the cache</typeparam>
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
