namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;

    /// <summary>
    /// List of possible histogram aggregations.
    /// </summary>
    [Flags]
    public enum MeterAggregations
    {
        /// <summary>
        /// Rate aggregation.
        /// </summary>
        Rate = 0x1,

        /// <summary>
        /// Sum aggregation.
        /// </summary>
        Sum = 0x2
    }
}
