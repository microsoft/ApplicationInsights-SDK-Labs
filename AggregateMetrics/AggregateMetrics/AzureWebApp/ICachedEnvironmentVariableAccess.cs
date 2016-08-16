namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    /// <summary>
    /// Interface for classes that implement a CacheHelper.
    /// </summary>
    public interface ICachedEnvironmentVariableAccess
    {
        /// <summary>
        /// Returns value of a counter from cache.
        /// </summary>
        /// <param name="name"> Name of the counter.</param>
        /// <returns> Counter value.</returns>
        int GetCounterValue(string name, AzureWebApEnvironmentVariables environmentVariable);
    }
}
