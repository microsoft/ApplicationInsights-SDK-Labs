namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using System.Threading;

    /// <summary>
    /// Telemetry module that sends aggregated metrics to the backend periodically.
    /// </summary>
    public class AggregateMetricsTelemetryModule : ITelemetryModule
    {
        private TelemetryConfiguration configuration;

        private TelemetryClient telemetryClient;

        private TimeSpan flushInterval = Constants.DefaultTimerFlushInterval;

        private Thread aggregationThread;

        private void WorkerThread()
        {
            while (true)
            {
                Thread.Sleep(this.FlushInterval);

                foreach (var counter in this.configuration.GetCounters())
                {
                    this.telemetryClient.TrackMetric(counter.GetValueAndReset());
                }
            }
        }

        /// <summary>
        /// Metrics aggregation interval.
        /// </summary>
        public TimeSpan FlushInterval
        {
            get
            {
                return flushInterval;
            }
            set
            {
                if (value < Constants.MinimumTimerFlushInterval || value > Constants.MaximumTimerFlushInterval)
                {
                    AggregateMetricsEventSource.Log.FlushIntervalSecondsOutOfRange(value.TotalSeconds);

                    if (value < Constants.MinimumTimerFlushInterval)
                    {
                        this.flushInterval = Constants.MinimumTimerFlushInterval;
                    }

                    if (value > Constants.MaximumTimerFlushInterval)
                    {
                        this.flushInterval = Constants.MaximumTimerFlushInterval;
                    }
                }
                else
                {
                    flushInterval = value;
                }
            }
        }

        /// <summary>
        /// Initialized telemetry module - starts the timer.
        /// </summary>
        /// <param name="configuration">Telemetry configuration to aggregate counters from.</param>
        public void Initialize(TelemetryConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            AggregateMetricsEventSource.Log.ModuleInitializationBegin();

            var sdkVersion = SdkVersionUtils.VersionPrefix + SdkVersionUtils.GetAssemblyVersion();

            this.configuration = configuration;
            this.telemetryClient = new TelemetryClient(this.configuration);
            this.telemetryClient.Context.GetInternalContext().SdkVersion = sdkVersion;

            aggregationThread = new Thread(new ThreadStart(WorkerThread)) { IsBackground = true };
            aggregationThread.Start();

            AggregateMetricsEventSource.Log.ModuleInitializationEnd();
        }
    }
}
