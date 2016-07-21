namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using System.Net.Http;
    using System.Runtime.Caching;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
 
    /// <summary>
    /// Gauge that gives the user an aggregate of requested counters in a cache
    /// </summary>
    public class FlexiblePerformanceCounterGauge 
    {
        /// <summary>
        /// Retrieves raw counter data from Environment Variables
        /// </summary>
        /// <returns> string version of raw JSON </returns>
        public string GetJson()
        {
            HttpClient client = new HttpClient();

            Task<string> counterRetrieval = client.GetStringAsync("http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/"); 
            counterRetrieval.Wait();

            return counterRetrieval.Result;
        }

        /// <summary>
        /// Cleans raw JSON for only requested counter 
        /// </summary>
        /// <param name="name"> Camel case name of sought counter</param>
        /// <param name="json"> string version of raw JSON</param>
        /// <returns> String representation of counter value</returns>
        public string GetValue(string name, string json)
        {
            string prefix = "\\\"" + name + "\\\":";
            string postfix = ",\\";

            int idx = json.IndexOf(prefix) + prefix.Length;
            int endIdx = json.IndexOf(postfix, idx);

            return json.Substring(idx, endIdx - idx);
        }

        /// <summary>
        /// Main method to cache raw JSON and process it for raw output
        /// </summary>
        /// <param name="name"> Camel case name of sought counter</param>
        /// <returns> Metric Telemetry object mt, with values for Name and Value </returns>
        public MetricTelemetry GetRawCounterValue(string name)
        {
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(5.0);
            
             /* http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ASPNET/
             http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/
             http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_CLR/
             http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ALL/ */

            if (cache[name] == null)
            {
                string json = this.GetJson();
                cache.Set(name, json, DateTimeOffset.Now.AddSeconds(5.0));
            }

            string cachedJson = (string)cache.Get(name);

            string value = this.GetValue(name, cachedJson);

            var regularExpressions = new Regex(
                @"(?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", 
                 RegexOptions.IgnorePatternWhitespace);

            MetricTelemetry mt = new MetricTelemetry();
            mt.Name = regularExpressions.Replace(name, " ");
            mt.Value = Convert.ToInt32(value);

            return mt;
                   }
    }
}
