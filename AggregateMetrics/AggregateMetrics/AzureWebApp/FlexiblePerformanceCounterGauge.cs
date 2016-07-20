using System;
using Microsoft.ApplicationInsights.DataContracts;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    class FlexiblePerformanceCounterGauge
    {
        public string GetJson()
        {
            HttpClient client = new HttpClient();
            var t = client.GetStringAsync("http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/");
            t.Wait();

            return t.Result;
        }

        public string GetValue(string name, string json)
        {
            string prefix = "\\\""+name+"\\\":";
            string postfix = ",\\";

            int idx = json.IndexOf(prefix) + prefix.Length;
            int endIdx = json.IndexOf(postfix, idx);

            return json.Substring(idx, endIdx - idx);
        }

        public MetricTelemetry GetRawCounterValue(string name)
        {
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(5.0);

            //http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ASPNET/
            //http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/
            //http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_CLR/
            //http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ALL/

            if (cache["jsonContent"] == null)
            {
                string json = GetJson();
                cache.Set("jsonContent", json, DateTimeOffset.Now.AddSeconds(5.0));
            }

            string cachedJson = (string)cache.Get("jsonContent");

            string value = GetValue(name,cachedJson);

            var regularExpressions = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);


            MetricTelemetry mt = new MetricTelemetry();
            mt.Name = regularExpressions.Replace(name, " ");
            mt.Value = Convert.ToInt32(value);

            return mt;
            ;       }
    }
}
