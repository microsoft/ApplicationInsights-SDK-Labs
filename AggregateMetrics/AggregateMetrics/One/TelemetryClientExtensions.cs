using System;
using System.Reflection;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
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
            var counter = new CounterImplementation();

            var configuration = GetConfigurationFromClient(telemetryClient);

            configuration.RegisterCounter(name, counter);

            return counter;
        }

        public static void Gauge(this TelemetryClient telemetryClient, string name, Func<long> valueFunc)
        {
            var gauge = new GaugeImplementation(valueFunc);

            var configuration = GetConfigurationFromClient(telemetryClient);

            configuration.RegisterCounter(name, gauge);
        }


    }
}
