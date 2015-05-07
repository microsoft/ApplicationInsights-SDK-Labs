namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics
{
    using System;

    /// <summary>
    /// Aggregate metrics telemetry module.
    /// </summary>
    public class AggregateMetricsTelemetryModule : ISupportConfiguration
    {
        private static int flushIntervalSeconds = 15;

        /// <summary>
        /// Initialize the telemetry module.
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
        }

        /// <summary>
        /// The interval which to flush aggregate metrics into a MetricTelemetry item.
        /// </summary>
        public static int FlushIntervalSeconds
        {
            get { return flushIntervalSeconds; }
            set
            {
                if (value < 5)
                {
                    throw new ArgumentOutOfRangeException("The minimum flush interval is 5 seconds.");
                }

                if (value > 60)
                {
                    throw new ArgumentOutOfRangeException("The maximum flush interval is 60 seconds.");
                }

                flushIntervalSeconds = value;
            }
        }

        /// <summary>
        /// If automatic timer-based flush enabled.
        /// </summary>
        public static bool IsTimerFlushEnabled = true;
    }
}
