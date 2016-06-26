using System;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics
{
    /// <summary>
    /// Configuration constants.
    /// </summary>
    public static class Constants
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
        /// The default timer flush interval.
        /// </summary>
        public static TimeSpan DefaultTimerFlushInterval = TimeSpan.FromSeconds(15);

        /// <summary>
        /// The minimum timer flush interval.
        /// </summary>
        public static TimeSpan MinimumTimerFlushInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The maxmimum timer flush interval.
        /// </summary>
        public static TimeSpan MaximumTimerFlushInterval = TimeSpan.FromMinutes(2);

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
