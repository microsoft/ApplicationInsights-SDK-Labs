namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System.Text.RegularExpressions;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;

    /// <summary>
    /// Gauge that gives the user an aggregate of requested counters in a cache
    /// </summary>
    public class FlexiblePerformanceCounterGauge : ICounterValue
    {
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
            /// <returns> Metric Telemetry object mt, with values for Name and Value </returns>

            public MetricTelemetry GetValueAndReset()
            {

            var metric = new MetricTelemetry();
            var regularExpressions = new Regex(
                 @"(?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])",
                RegexOptions.IgnorePatternWhitespace);

            metric.Name = regularExpressions.Replace(name, " "); 
            metric.Value = CacheHelper.GetCountervalue(name);

            return metric;
            }
    }
}


