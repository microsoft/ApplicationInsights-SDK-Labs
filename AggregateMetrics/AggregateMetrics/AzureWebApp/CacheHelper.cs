namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using System.Net.Http;
    using System.Runtime.Caching;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to contain the one cache for all Gauges.
    /// </summary>
    internal class CacheHelper
    {
        private static readonly CacheHelper instance = new CacheHelper();

        static CacheHelper() { }

        private CacheHelper() { }

        /// <summary>
        /// Return the only instance of CacheHelper.
        /// </summary>
        public static CacheHelper Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Searchs for the value of a given performance counter in a json.
        /// </summary>
        /// <param name="performanceCounterName"> The name of the performance counter.</param>
        /// <param name="json"> String containing the json.</param>
        /// <returns> Value of the performance counter.</returns>
        public int PerformanceCounterValue(string performanceCounterName, string json)
        {
            string jsonSubstring = json.Substring(json.IndexOf(performanceCounterName), json.Length - json.IndexOf(performanceCounterName));

            int startingIdx = jsonSubstring.IndexOf(" ");
            int endingIdx = jsonSubstring.IndexOf(",");

            string value = jsonSubstring.Substring(startingIdx + 1, endingIdx - startingIdx - 1);

            return Convert.ToInt32(value);
        }

        /// <summary>
        /// Checks if a key is in the cache and if not
        /// Retrieves raw counter data from Environment Variables
        /// Cleans raw JSON for only requested counter
        /// Creates value for caching
        /// </summary>
        /// <param name="name">Cache key and name of the counter to be selected from JSON</param>
        /// <returns>value from cache</returns>
        public int GetCounterValue(string name)
        {
            const string jsonKey = "json";
            if (!CacheHelper.Instance.IsInCache(jsonKey))
            {
                PerformanceCounterImplementation client = new PerformanceCounterImplementation();
                string uncachedJson= client.GetAzureWebAppEnvironmentVariables(AzureWebApEnvironmentVariables.All);

                if (uncachedJson == null)
                {
                    return 0;
                }

                CacheHelper.Instance.SaveToCache(jsonKey, uncachedJson, DateTimeOffset.Now.AddSeconds(5.0));
            }

            string json = GetFromCache(jsonKey).ToString();
            int value = PerformanceCounterValue(name, json);

            return value;
        }

        /// <summary>
        /// Retrieves raw counter data from Environment Variables.
        /// This method is meant to be used only for testing.
        /// </summary>
        /// <param name="name"> Name of the counter to be selected from JSON.</param>
        /// <returns> Value of the counter.</returns>
        public int GetCounterValueHttp(string name)
        {
            /* http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ASPNET/
                http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/
                http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_CLR/
               http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ALL/ */

            HttpClient client = new HttpClient();
            Task<string> counterRetrieval = client.GetStringAsync("http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ALL/");
            counterRetrieval.Wait();

            string json = counterRetrieval.Result;
            int value = PerformanceCounterValue(name, json);

            return value;
        }

        /// <summary>
        /// Method saves an object to the cache
        /// </summary>
        /// <param name="cacheKey"> string name of the counter value to be saved to cache</param>
        /// /<param name="toCache">Object to be cached</param>
        /// <param name="absoluteExpiration"> DateTimeOffset until item expires from cache</param>
        public void SaveToCache(string cacheKey, object toCache,  DateTimeOffset absoluteExpiration)
        {  
            MemoryCache.Default.Add(cacheKey, toCache, absoluteExpiration);
        }

        /// <summary>
        /// Retrieves requested item from cache.
        /// </summary>
        /// <param name="cacheKey"> Key for the retrieved object.</param>
        /// <returns> The requested item, as object type T.</returns>
        public object GetFromCache(string cacheKey) 
        {
            return MemoryCache.Default[cacheKey];
        }

        /// <summary>
        /// Method to check if a key is in a cache
        /// </summary>
        /// <param name="cacheKey">key to search for in cache</param>
        /// <returns>Boolean value for whether or not a key is in the cache</returns>
        public bool IsInCache(string cacheKey)
        {
            return MemoryCache.Default[cacheKey] != null;
        }
    }
}
