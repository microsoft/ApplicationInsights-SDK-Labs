namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics
{
    internal struct AggregationResult
    {
        internal int Count { get; set; }

        internal double Sum { get; set; }

        internal double Min { get; set; }

        internal double Max { get; set; }

        internal double StdDev { get; set; }

        internal double Average
        {
            get
            {
                return Sum / Count;
            }
        }

        internal double P50 { get; set; }

        internal double P75 { get; set; }

        internal double P90 { get; set; }

        internal double P95 { get; set; }

        internal double P99 { get; set; }
    }
}
