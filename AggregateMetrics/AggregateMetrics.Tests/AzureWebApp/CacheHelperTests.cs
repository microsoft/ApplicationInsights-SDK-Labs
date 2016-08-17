using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AggregateMetrics.Tests.AzureWebApp
{
    internal class CacheHelperTests : ICachedEnvironmentVariableAccess
    {
        private bool returnJsonOne = true;

        private string jsonOne = File.ReadAllText(@"AzureWebApp\SampleFiles\RemoteEnvironmentVariablesAllSampleOne.txt");

        private string jsonTwo = File.ReadAllText(@"AzureWebApp\SampleFiles\RemoteEnvironmentVariablesAllSampleTwo.txt");

        /// <summary>
        /// Retrieves raw counter data from Environment Variables.
        /// </summary>
        /// <param name="name"> Name of the counter to be selected from JSON.</param>
        /// <returns> Value of the counter.</returns>
        public int GetCounterValue(string name, AzureWebApEnvironmentVariables environmentVariable)
        {
            if (returnJsonOne)
            {
                returnJsonOne = false;
                return CacheHelper.Instance.PerformanceCounterValue(name, jsonOne);
            }
            else
            {
                returnJsonOne = true;
                return CacheHelper.Instance.PerformanceCounterValue(name, jsonTwo);
            }
        }
    }
}