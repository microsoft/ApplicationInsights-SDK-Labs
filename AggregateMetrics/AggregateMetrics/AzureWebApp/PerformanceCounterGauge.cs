using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
using System;
using Microsoft.ApplicationInsights.DataContracts;
using System.Net.Http;
using System.Runtime.Caching; 

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{

    class PerformanceCounterGauge 

    {
        private string GetJson()
        {
            HttpClient client = new HttpClient();
            var t = client.GetStringAsync("http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_APP/");
            t.Wait();

            return t.Result;
        }

        private string GetValue(string json)
        {
            string prefix = "\\\"privateBytes\\\":";
            string postfix = ",\\";

            int idx = json.IndexOf(prefix) + prefix.Length;
            int endIdx = json.IndexOf(postfix, idx);

            return json.Substring(idx, endIdx - idx);
        }

        private MetricTelemetry GetRawCounterValue()
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

            string value = GetValue(cachedJson);
       
            MetricTelemetry mt = new MetricTelemetry();
            mt.Name = "Private bytes";
            mt.Value = Convert.ToInt32(value);

            return mt;
        }

    }

}


