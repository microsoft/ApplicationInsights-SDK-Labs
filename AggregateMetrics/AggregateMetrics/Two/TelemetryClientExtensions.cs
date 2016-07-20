namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;
    using System.Reflection;

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
        /// <returns>Returns the counter implementation.</returns>
        public static ICounter Counter(this TelemetryClient telemetryClient, string name)
        {
            var counter = new CounterImplementation(name, telemetryClient.Context);

            var configuration = GetConfigurationFromClient(telemetryClient);

            configuration.RegisterCounter(counter);

            return counter;
        }

        /// <summary>
        /// Gauges is a simple metric type that takes the value from the delegate. 
        /// It can be used to track the value of performance counter or queue size. 
        /// </summary>
        /// <param name="telemetryClient">Telemetry client to get Gauge from.</param>
        /// <param name="name">Name of the gauge.</param>
        /// <param name="valueFunc">Callback function to return the gauge value.</param>
        public static void Gauge(this TelemetryClient telemetryClient, string name, Func<int> valueFunc)
        {
            var gauge = new GaugeImplementation(name, telemetryClient.Context, valueFunc);

            var configuration = GetConfigurationFromClient(telemetryClient);

            configuration.RegisterCounter(gauge);
        }

        /// <summary>
        /// Meter represents the metric that measurese the rate at which an event occurs. 
        /// You can use meter to count failed requests per second metric.
        /// </summary>
        /// <param name="telemetryClient">Telemetry client to associate the meter with.</param>
        /// <param name="name">Name of the meter.</param>
        /// <param name="aggregations">Aggregation to apply.</param>
        /// <returns>Returns a meter implementation.</returns>
        public static IMeter Meter(this TelemetryClient telemetryClient, string name, MeterAggregations aggregations = MeterAggregations.Rate)
        {
            var meter = new MeterImplementation(name, telemetryClient.Context, aggregations);

            var configuration = GetConfigurationFromClient(telemetryClient);
            configuration.RegisterCounter(meter);

            return meter;
        }

        /// <summary>
        /// Histogram represents an aggregated counter on the stream of values. It can calculate min, mean, max and stdDeviation.
        /// </summary>
        /// <param name="telemetryClient">Telemetry client to associate the meter with.</param>
        /// <param name="name">Name of the histogram.</param>
        /// <param name="aggregations">Types of aggregations to perform.</param>
        /// <returns>Returns a histogram implementation.</returns>
        public static IHistogram Histogram(this TelemetryClient telemetryClient, string name, HistogramAggregations aggregations = HistogramAggregations.Mean | 
            HistogramAggregations.MinMax |
            HistogramAggregations.Percentiles)
        {
            var histogram = new HistogramImplementation(name, telemetryClient.Context, aggregations);

            var configuration = GetConfigurationFromClient(telemetryClient);
            configuration.RegisterCounter(histogram);

            return histogram;
        }
    }
}
