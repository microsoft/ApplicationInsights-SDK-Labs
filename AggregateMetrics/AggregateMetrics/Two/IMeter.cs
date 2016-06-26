namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    /// <summary>
    /// Meter represents the metric that measurese the rate at which an event occurs. 
    /// You can use meter to count failed requests per second metric.
    /// </summary>
    public interface IMeter
    {
        /// <summary>
        /// Mark an event.
        /// </summary>
        void Mark();

        /// <summary>
        /// Mark an event occurence count times.
        /// </summary>
        void Mark(int count);
    }
}
