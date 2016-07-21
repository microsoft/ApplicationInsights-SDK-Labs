namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPerformanceCounter
    {
        /// <summary>
        /// Retrieves counter data from Azure Web App Environment Variables.
        /// </summary>
        /// <param name="environmentVariable">Name of environment variable</param>
        /// <returns>Raw Json with</returns>
        string GetAzureWebAppEnvironmentVariables(AzureWebApEnvironmentVariables environmentVaribale);
    }
}
