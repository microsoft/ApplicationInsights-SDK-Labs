namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
{
    using System;

    /// <summary>
    /// Aggregate metrics telemetry module.
    /// </summary>
    public class AggregateMetricsTelemetryModule : ITelemetryModule
    {
        private static TimeSpan _flushIntervalSeconds = Constants.DefaultTimerFlushInterval;
        private static bool _isTimerFlushEnabledUser = true;
        private static bool _isTimerFlushEnabledInternal = true;

        /// <summary>
        /// Initialize the telemetry module.
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
            FlushInterval = Constants.DefaultTimerFlushInterval;
        }

        /// <summary>
        /// The interval which to flush aggregate metrics into a MetricTelemetry item.
        /// </summary>
        public static TimeSpan FlushInterval
        {
            get { return _flushIntervalSeconds; }
            set
            {
                if (value < Constants.MinimumTimerFlushInterval || value > Constants.MaximumTimerFlushInterval)
                {
                    _isTimerFlushEnabledInternal = false;
                    AggregateMetricsEventSource.Log.FlushIntervalSecondsOutOfRange(value);
                }
                else
                {
                    _flushIntervalSeconds = value;
                }
            }
        }

        /// <summary>
        /// If automatic timer-based flush enabled.
        /// </summary>
        public static bool IsTimerFlushEnabled
        {
            get
            {
                return _isTimerFlushEnabledUser && _isTimerFlushEnabledInternal;
            }
            set
            {
                _isTimerFlushEnabledUser = value;
            }
        }
    }
}
