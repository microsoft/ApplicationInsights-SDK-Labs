namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
{
    /// <summary>
    /// How the percentiles should be calculated.
    /// </summary>
    public enum PercentileCalculation
    {
        /// <summary>
        /// Do not calculation any percentiles.
        /// </summary>
        DoNotCalculate,

        /// <summary>
        /// Calculate the percentiles ordering by largest.
        /// </summary>
        OrderByLargest,

        /// <summary>
        /// Calculate the percentiles ordering by smallest.
        /// </summary>
        OrderBySmallest
    }
}
