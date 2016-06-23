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

        private string sdkVersion;

        private TimeSpan flushInterval = TimeSpan.FromSeconds(Constants.DefaultTimerFlushInterval);

        //private static System.Threading.Timer aggregationTimer;

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

        private void TimerFlushCallback(object obj)
        {
            foreach(var counter in this.configuration.GetCounters())
            {
                this.telemetryClient.TrackMetric(counter.GetValueAndReset());
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
                //TODO: clear up the nonsense with timespan and int conversion by unifying approach with "One"
                if (value < TimeSpan.FromSeconds(Constants.MinimumTimerFlushInterval) || value > TimeSpan.FromSeconds(Constants.MaximumTimerFlushInterval))
                {
                    AggregateMetricsEventSource.Log.FlushIntervalSecondsOutOfRange(Convert.ToInt32(value.TotalSeconds));

                    if (value < TimeSpan.FromSeconds(Constants.MinimumTimerFlushInterval))
                    {
                        this.flushInterval = TimeSpan.FromSeconds(Constants.MinimumTimerFlushInterval);
                    }

                    if (value > TimeSpan.FromSeconds(Constants.MaximumTimerFlushInterval))
                    {
                        this.flushInterval = TimeSpan.FromSeconds(Constants.MaximumTimerFlushInterval);
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

            sdkVersion = SdkVersionUtils.VersionPrefix + SdkVersionUtils.GetAssemblyVersion();

            this.configuration = configuration;
            this.telemetryClient = new TelemetryClient(this.configuration);
            this.telemetryClient.Context.GetInternalContext().SdkVersion = sdkVersion;

            AggregateMetricsEventSource.Log.ModuleInitializationStarted();

            //aggregationTimer = new System.Threading.Timer(new System.Threading.TimerCallback(this.TimerFlushCallback), null, this.FlushInterval, this.FlushInterval);

            aggregationThread = new Thread(new ThreadStart(WorkerThread)) { IsBackground = true };
            aggregationThread.Start();
        }
    }
}
