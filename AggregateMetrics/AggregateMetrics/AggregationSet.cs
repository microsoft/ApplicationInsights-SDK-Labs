namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Stores a set of aggregations using TelemetryClient|name|property names as the key.
    /// </summary>
    internal class AggregationSet
    {
        internal ConcurrentDictionary<string, string> property1Values = null;
        internal ConcurrentDictionary<string, string> property2Values = null;
        internal ConcurrentDictionary<string, string> property3Values = null;

        internal ConcurrentDictionary<int, MetricsBag> aggregations = new ConcurrentDictionary<int,MetricsBag>();

        internal AggregationSet(TelemetryClient telemetryClient, string name)
        {
            this.TelemetryClient = telemetryClient;
            this.Name = name;
        }

        internal TelemetryClient TelemetryClient { get; private set; }

        internal int Key
        {
            get
            {
                return GetKey(this.TelemetryClient, this.Name);
            }
        }

        internal string Name { get; private set; }

        internal void AddAggregation(double value, string property1 = null, string property2 = null, string property3 = null)
        {
            if (!string.IsNullOrWhiteSpace(property1))
            {
                if (this.property1Values == null)
                {
                    this.property1Values = new ConcurrentDictionary<string, string>();
                }

                property1 = GetPropertyValue(this.property1Values, property1);
            }

            if (!string.IsNullOrWhiteSpace(property2))
            {
                if (this.property2Values == null)
                {
                    this.property2Values = new ConcurrentDictionary<string, string>();
                }

                property2 = GetPropertyValue(this.property2Values, property2);
            }

            if (!string.IsNullOrWhiteSpace(property3))
            {
                if (this.property3Values == null)
                {
                    this.property3Values = new ConcurrentDictionary<string, string>();
                }

                property3 = GetPropertyValue(this.property3Values, property3);
            }

            int aggregationKey = GetAggregationKey(property1, property2, property3);

            MetricsBag counterData = this.aggregations.GetOrAdd(aggregationKey, (key) =>
                {
                    return new MetricsBag(property1, property2, property3);
                });

            Debug.Assert(this.aggregations.Count <= (Constants.MaxPropertyCardinality + 2) * (Constants.MaxPropertyCardinality + 2) * (Constants.MaxPropertyCardinality + 2));

            counterData.Add(value);
        }

        internal ConcurrentDictionary<int, MetricsBag> RemoveAggregations()
        {
            var newAggregations = new ConcurrentDictionary<int, MetricsBag>();

            return Interlocked.Exchange(ref this.aggregations, newAggregations);
        }

        internal static int GetKey(TelemetryClient telemetryClient, string name)
        {
            return CombineHashCodes(telemetryClient.GetHashCode(), name.GetHashCode());
        }

        private static int GetAggregationKey(string property1, string property2, string property3)
        {
            int h1 = 0;
            if (property1 != null)
            {
                h1 = property1.GetHashCode();
            }

            int h2 = 0;
            if (property2 != null)
            {
                h2 = property2.GetHashCode();
            }

            int h3 = 0;
            if (property3 != null)
            {
                h3 = property3.GetHashCode();
            }

            return CombineHashCodes(CombineHashCodes(h1, h2), h3);
        }

        // From System.Web.Util.HashCodeCombiner
        private static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        private static string GetPropertyValue(ConcurrentDictionary<string, string> dict, string property)
        {
            if (property.Length > Constants.NameMaxLength)
            {
                property = property.Substring(0, Constants.NameMaxLength);
            }

            if (dict.Count < Constants.MaxPropertyCardinality)
            {
                return dict.GetOrAdd(property, property);
            }

            string dictValue;
            if (dict.TryGetValue(property, out dictValue))
            {
                return dictValue;
            }

            // If it's not in the dict and the dict is over max length bucket into other.
            return "other";
        }
    }
}
