using Microsoft.ApplicationInsights.DataContracts;
using System;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    /// <summary>
    /// Struct for metrics dependant on time.
    /// </summary>
    public struct RateCounter
    {
        /// <summary>
        /// Metric object.
        /// </summary>
        public MetricTelemetry MetricTelemetry;

        /// <summary>
        /// DateTime object to keep track of the last time this metric was retrieved.
        /// </summary>
        public DateTime DateTime;
    }
}
