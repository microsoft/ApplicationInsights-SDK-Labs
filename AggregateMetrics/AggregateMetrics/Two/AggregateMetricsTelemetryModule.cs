namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;

    public class AggregateMetricsTelemetryModule : ITelemetryModule
    {
        private TelemetryConfiguration configuration;

        private TelemetryClient telemetryClient;

        private string sdkVersion;

        private TimeSpan flushInterval = TimeSpan.FromSeconds(Constants.DefaultTimerFlushInterval);

        private static System.Threading.Timer aggregationTimer;

        private void TimerFlushCallback(object obj)
        {
            foreach(var counter in this.configuration.GetCounters())
            {
                this.telemetryClient.TrackMetric(counter.GetValueAndReset());
            }
        }

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

        public void Initialize(TelemetryConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            this.configuration = configuration;
            this.telemetryClient = new TelemetryClient(this.configuration);

            AggregateMetricsEventSource.Log.ModuleInitializationStarted();
            sdkVersion = SdkVersionUtils.VersionPrefix + SdkVersionUtils.GetAssemblyVersion();

            aggregationTimer = new System.Threading.Timer(new System.Threading.TimerCallback(this.TimerFlushCallback), null, this.FlushInterval, this.FlushInterval);
        }
    }
}
