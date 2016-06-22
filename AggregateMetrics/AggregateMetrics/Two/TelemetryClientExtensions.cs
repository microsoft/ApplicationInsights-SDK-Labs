using System;
using System.Reflection;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    /// <summary>
    /// Extensions for <cref c="TelemetryClient" /> for aggregated counters.
    /// </summary>
    public static class TelemetryClientExtensions
    {
        private static TelemetryConfiguration GetConfigurationFromClient(TelemetryClient telemetryClient)
        {
            // This is a hack. It will go away when this code will become a part of Application Insights Core
            return (TelemetryConfiguration)typeof(TelemetryClient).GetField("configuration", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(telemetryClient);
        }

        /// <summary>
        /// Registers the new counter object.
        /// </summary>
        /// <param name="telemetryClient">Telemetry client to associate the counter with.</param>
        /// <param name="name">Name of the counter.</param>
        /// <returns></returns>
        public static ICounter Counter(this TelemetryClient telemetryClient, string name)
        {
            var counter = new CounterImplementation(name, telemetryClient.Context);

            var configuration = GetConfigurationFromClient(telemetryClient);

            configuration.RegisterCounter(counter);

            return counter;
        }

        public static void Gauge(this TelemetryClient telemetryClient, string name, Func<long> valueFunc)
        {
            var gauge = new GaugeImplementation(name, telemetryClient.Context, valueFunc);

            var configuration = GetConfigurationFromClient(telemetryClient);

            configuration.RegisterCounter(gauge);
        }

        public static IMeter Meter(this TelemetryClient telemetryClient, string name)
        {
            var meter = new MeterImplementation(name, telemetryClient.Context);

            var configuration = GetConfigurationFromClient(telemetryClient);
            configuration.RegisterCounter(meter);

            return meter;
        }
    }
}
