namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;

    public class AggregateMetricsTelemetryModule : ITelemetryModule
    {
        private TelemetryConfiguration configuration;

        private TelemetryClient telemetryClient;

        private string sdkVersion;


        private static System.Threading.Timer aggregationTimer;

        private void TimerFlushCallback(object obj)
        {
            foreach(var counter in this.configuration.GetCounters())
            {
                this.telemetryClient.TrackMetric(counter.Value.GetValueAndReset());
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

            aggregationTimer = new System.Threading.Timer(new System.Threading.TimerCallback(this.TimerFlushCallback), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
    }
}
