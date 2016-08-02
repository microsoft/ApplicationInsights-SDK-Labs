using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    class DefaultCounters
    {
        Dictionary<FlexiblePerformanceCounterGauge, MetricTelemetry> defaultCounters;

        /// <summary>
        /// Initializes a dictionary for the default performance counters.
        /// </summary>
        public void Initialize()
        {
            defaultCounters = new Dictionary<FlexiblePerformanceCounterGauge, MetricTelemetry>()
            {
                {  new FlexiblePerformanceCounterGauge("appRequestExecTime"), new MetricTelemetry() },
                {  new FlexiblePerformanceCounterGauge("privateBytes"), new MetricTelemetry() },
                {  new FlexiblePerformanceCounterGauge("requestsInApplicationQueue"), new MetricTelemetry() }
            };
        }

        /// <summary>
        /// Gets the default performance counters.
        /// </summary>
        /// <returns> List of MetricTelemetry objects</returns>
        public List<MetricTelemetry> GetCounters()
        {
            List<MetricTelemetry> metrics = new List<MetricTelemetry>();

            foreach (var item in defaultCounters.ToList())
            {
                defaultCounters[item.Key] = item.Key.GetValueAndResetHttp();
            }

            foreach (MetricTelemetry metric in defaultCounters.Values.ToList())
            {
                metrics.Add(metric);
            }

            return metrics;
        }
    }
}
