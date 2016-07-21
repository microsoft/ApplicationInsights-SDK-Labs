using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AggregateMetrics.Tests.AzureWebApp
{
    [TestClass]
    public class GetEnvironmentVariablesHttpTest : IPerformanceCounter
    {
        [TestMethod]
        public string GetAzureWebAppEnvironmentVariables(AzureWebApEnvironmentVariables environmentVaribale)
        {
            HttpClient client = new HttpClient();

            Task<string> task = client.GetStringAsync("http://remoteenvironmentvariables.azurewebsites.net/api/EnvironmentVariables/WEBSITE_COUNTERS_ASPNET/");

            task.Wait();

            string json = task.Result;

            Assert.IsTrue(json.Contains("applicationRestarts"));

            return json;
        }
    }
}
