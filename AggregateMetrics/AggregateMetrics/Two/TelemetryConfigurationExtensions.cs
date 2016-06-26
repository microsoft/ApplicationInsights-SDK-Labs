namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    
    /// <summary>
    /// Extensions methods for telemetry configuration
    /// </summary>
    public static class TelemetryConfigurationExtensions
    {
        private static ConditionalWeakTable<TelemetryConfiguration, List<ICounterValue>> counters = new ConditionalWeakTable<TelemetryConfiguration, List<ICounterValue>>();

        private static List<ICounterValue> CreateEmptyDictionary(TelemetryConfiguration configuration)
        {
            return new List<ICounterValue>();
        }

        /// <summary>
        /// Registers counter for periodic extraction into telemetry configuration.
        /// </summary>
        /// <param name="configuration">Telemetry configuration to store the counter.</param>
        /// <param name="counter">Counter vaue interface implementation.</param>
        public static void RegisterCounter(this TelemetryConfiguration configuration, ICounterValue counter)
        {
            var countersCollection = counters.GetValue(configuration, CreateEmptyDictionary);
            countersCollection.Add(counter);
        }

        /// <summary>
        /// Returns the list of counters stored into configuration.
        /// </summary>
        /// <param name="configuration">Telemetry configuration that stores the counters collection.</param>
        public static IList<ICounterValue> GetCounters(this TelemetryConfiguration configuration)
        {
            return counters.GetValue(configuration, CreateEmptyDictionary);
        }

    }
}
