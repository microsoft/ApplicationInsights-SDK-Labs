namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    /// <summary>
    /// Histogram metric. Aggregation on stream of value.
    /// </summary>
    public interface IHistogram
    {
        /// <summary>
        /// Next value in the stream of values to aggregate.
        /// </summary>
        /// <param name="value">Value to aggregate.</param>
        void Update(int value);
    }
}
