namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    /// <summary>
    /// Counter represents an integer value that can be incremented or decremented. 
    /// You can use this metric type to count the number of worker threads or some business metric.
    /// </summary>
    public interface ICounter
    {
        /// <summary>
        /// Increment counter by 1.
        /// </summary>
        void Increment();

        /// <summary>
        /// Increment counter by the value passed as an argument.
        /// </summary>
        /// <param name="value">Value to which increment a counter.</param>
        void Increment(long value);

        /// <summary>
        /// Decrement counter by 1.
        /// </summary>
        void Decrement();

        /// <summary>
        /// Decrement counter by the value passed as an argument.
        /// </summary>
        /// <param name="value">Value to which decrement a counter.</param>
        void Decrement(long value);
    }
}
