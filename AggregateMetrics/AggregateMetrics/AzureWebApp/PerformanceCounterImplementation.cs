using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    class PerformanceCounterImplementation : IPerformanceCounter
    {
        /// <summary>
        /// Available Environment Variables in Azure Web Apps
        /// </summary>
        private readonly List<string> listEnvironmentVariables = new List<string>
        {
            "WEBSITE_COUNTERS_ASPNET",
            "WEBSITE_COUNTERS_APP",
            "WEBSITE_COUNTERS_CLR",
            "WEBSITE_COUNTERS_ALL"
        };

        /// <summary>
        /// Retrieves counter data from Azure Web App Environment Variables.
        /// </summary>
        /// <param name="environmentVariable">Name of environment variable</param>
        /// <returns>Raw Json with</returns>
        public string GetAzureWebAppEnvironmentVariables(AzureWebApEnvironmentVariables environmentVariable)
        {
            return Environment.GetEnvironmentVariable(listEnvironmentVariables[(int)environmentVariable]);
        }
    }
}
