namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;

    /// <summary>
    /// Gauge that gives the user an aggregate of requested counters in a cache
    /// </summary>
    public class FlexiblePerformanceCounterGauge : ICounterValue
    {
        /// <summary>
        /// Name of the counter variable to be used as cache key
        /// </summary>
        private string name;

        /// <summary>
        /// Implements name variable
        /// </summary>
        /// <param name="name">Name of counter variable</param>
        public FlexiblePerformanceCounterGauge(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Returns the current value of the counter as a <c ref="MetricTelemetry"/> and resets the metric.
        /// </summary>
        /// <returns> Metric Telemetry object, with values for Name and Value </returns>
        public MetricTelemetry GetValueAndReset()
        {
            var metric = new MetricTelemetry();

            metric.Name = this.name;
            metric.Value = CacheHelper.Instance.GetCounterValue(this.name);

            return metric;
        }
    }
}
