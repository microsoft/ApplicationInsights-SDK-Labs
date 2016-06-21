namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
{
    /// <summary>
    /// Configuration constants.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// Maximum length of the counter and property names.
        /// </summary>
        public const int NameMaxLength = 20;

        /// <summary>
        /// Maximum length of the property values.
        /// </summary>
        public const int PropertyValueMaxLength = 16;

        /// <summary>
        /// Maximum number of property names and values.
        /// </summary>
        public const int MaxPropertyCardinality = 5;

        /// <summary>
        /// Minimum number of items to calculate percentiles.
        /// </summary>
        public const int PercentileMinimumCount = 100;

        /// <summary>
        /// The default timer flush interval in seconds.
        /// </summary>
        public const int DefaultTimerFlushInterval = 15;

        /// <summary>
        /// The minimum timer flush interval in seconds.
        /// </summary>
        public const int MinimumTimerFlushInterval = 5;

        /// <summary>
        /// The maxmimum timer flush interval in seconds.
        /// </summary>
        public const int MaximumTimerFlushInterval = 60;

        internal const string DefaultP1Name = "p1";

        internal const string DefaultP2Name = "p2";

        internal const string DefaultP3Name = "p3";

        internal const string P50Name = "p50";

        internal const string P75Name = "p75";

        internal const string P90Name = "p90";

        internal const string P95Name = "p95";

        internal const string P99Name = "p99";
    }
}
