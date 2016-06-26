namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;

    /// <summary>
    /// List of possible histogram aggregations.
    /// </summary>
    [Flags]
    public enum HistogramAggregations
    {
        /// <summary>
        /// Minimal aggregation is a mean calculation.
        /// </summary>
        Mean = 0x0,

        /// <summary>
        /// Calculate minimum and maximum.
        /// </summary>
        MinMax = 0x1,

        /// <summary>
        /// Calculate standard deviation.
        /// </summary>
        StdDev = 0x2,

        /// <summary>
        /// Calculate 50th percentile.
        /// </summary>
        Percentile50 = 0x4
    }
}
