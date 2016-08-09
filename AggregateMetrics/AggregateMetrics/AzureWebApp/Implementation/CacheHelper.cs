namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using System.Net.Http;
    using System.Runtime.Caching;
    using System.Threading.Tasks;

    /// <summary>
    /// Class to contain the one cache for all Gauges.
    /// </summary>
    internal class CacheHelper : ICachedEnvironmentVariableAccess
    {
        /// <summary>
        /// Only instance of CacheHelper.
        /// </summary>
        private static readonly CacheHelper Instance = new CacheHelper();

        /// <summary>
        /// Prevents a default instance of the <see cref="CacheHelper"/> class from being created.
        /// </summary>
        private CacheHelper()
        {
        }

        /// <summary>
        /// Gets the only instance of CacheHelper.
        /// </summary>
        public static CacheHelper GetInstance
        {
            get
            {
                return Instance;
            }
        }

        /// <summary>
        /// Search for the value of a given performance counter in a Json.
        /// </summary>
        /// <param name="performanceCounterName"> The name of the performance counter.</param>
        /// <param name="json"> String containing the Json.</param>
        /// <returns> Value of the performance counter.</returns>
        public int PerformanceCounterValue(string performanceCounterName, string json)
        {
            if (json.IndexOf(performanceCounterName) == -1)
            {
                throw new System.ArgumentException("Counter was not found.", performanceCounterName);
            }

            string jsonSubstring = json.Substring(json.IndexOf(performanceCounterName), json.Length - json.IndexOf(performanceCounterName));

            int startingIndex = jsonSubstring.IndexOf(" ") + 1;
            int value;
            string valueString = string.Empty;

            while (char.IsDigit(jsonSubstring[startingIndex]))
            {
                valueString += jsonSubstring[startingIndex];
                startingIndex++;
            }

            if (!int.TryParse(valueString, out value))
            {
                throw new System.InvalidCastException("The value of the counter cannot be converted to integer type.");
            }

            return value;
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
            const string JonKey = "json";
            if (!CacheHelper.Instance.IsInCache(JonKey))
            {
                PerformanceCounterImplementation client = new PerformanceCounterImplementation();
                string uncachedJson = client.GetAzureWebAppEnvironmentVariables(AzureWebApEnvironmentVariables.All);

                if (uncachedJson == null)
                {
                    return 0;
                }

                CacheHelper.Instance.SaveToCache(JonKey, uncachedJson, DateTimeOffset.Now.AddSeconds(5.0));
            }

            string json = this.GetFromCache(JonKey).ToString();
            int value = this.PerformanceCounterValue(name, json);

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
