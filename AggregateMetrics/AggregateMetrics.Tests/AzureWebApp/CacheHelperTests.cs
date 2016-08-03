using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
using System.Net.Http;
using System.Threading.Tasks;

namespace AggregateMetrics.Tests.AzureWebApp
{
    internal class CacheHelperTests
    {
        private static readonly CacheHelperTests instance = new CacheHelperTests();

        private CacheHelperTests() { }

        /// <summary>
        /// Return the only instance of CacheHelper.
        /// </summary>
        public static CacheHelperTests Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Retrieves raw counter data from Environment Variables.
        /// </summary>
        /// <param name="name"> Name of the counter to be selected from JSON.</param>
        /// <returns> Value of the counter.</returns>
        public int GetCounterValue(string name)
        {
            /* http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ASPNET/
                http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/
                http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_CLR/
               http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ALL/ */

            HttpClient client = new HttpClient();
            Task<string> counterRetrieval = client.GetStringAsync("http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ALL/");
            counterRetrieval.Wait();

            string json = counterRetrieval.Result;
            int value = CacheHelper.Instance.PerformanceCounterValue(name, json);

            return value;
        }
    }
}
