namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    
    /// <summary>
    /// Extensions methods for telemetry configuration
    /// </summary>
    public static class TelemetryConfigurationExtensions
    {
        private static ConditionalWeakTable<TelemetryConfiguration, Dictionary<string, ICounterValue>> counters = new ConditionalWeakTable<TelemetryConfiguration, Dictionary<string, ICounterValue>>();

        private static Dictionary<string, ICounterValue> CreateEmptyDictionary(TelemetryConfiguration configuration)
        {
            return new Dictionary<string, ICounterValue>();
        }

        /// <summary>
        /// Registers counter for periodic extraction into telemetry configuration.
        /// </summary>
        /// <param name="configuration">Telemetry configuration to store the counter.</param>
        /// <param name="name">Name of the counter.</param>
        /// <param name="counter">Counter vaue interface implementation.</param>
        public static void RegisterCounter(this TelemetryConfiguration configuration, string name, ICounterValue counter)
        {
            var countersCollection = counters.GetValue(configuration, CreateEmptyDictionary);
            countersCollection.Add(name, counter);
        }

        /// <summary>
        /// Returns the list of counters stored into configuration.
        /// </summary>
        /// <param name="configuration">Telemetry configuration that stores the counters collection.</param>
        public static IDictionary<string, ICounterValue> GetCounters(this TelemetryConfiguration configuration)
        {
            return counters.GetValue(configuration, CreateEmptyDictionary);
        }

    }
}
