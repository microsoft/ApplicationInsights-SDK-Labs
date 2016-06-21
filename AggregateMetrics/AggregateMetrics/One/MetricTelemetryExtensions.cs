namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
{
    using System.Globalization;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.Implementation; 

    internal static class MetricTelemetryExtensions
    {
        private const string PropertyFormat = "{0}_{1}";

        internal static void AddProperties(this MetricTelemetry metricTelemetry, MetricsBag metricsBag, string p1Name, string p2Name, string p3Name)
        {
            if (metricsBag.Property1 != null)
            {
                if (p1Name == null)
                {
                    p1Name = string.Format(CultureInfo.InvariantCulture, PropertyFormat, metricTelemetry.Name, Constants.DefaultP1Name);
                }

                metricTelemetry.Properties[p1Name] = metricsBag.Property1;
            }

            if (metricsBag.Property2 != null)
            {
                if (p2Name == null)
                {
                    p2Name = string.Format(CultureInfo.InvariantCulture, PropertyFormat, metricTelemetry.Name, Constants.DefaultP2Name);
                }

                metricTelemetry.Properties[p2Name] = metricsBag.Property2;
            }

            if (metricsBag.Property3 != null)
            {
                if (p3Name == null)
                {
                    p3Name = string.Format(CultureInfo.InvariantCulture, PropertyFormat, metricTelemetry.Name, Constants.DefaultP3Name);
                }

                metricTelemetry.Properties[p3Name] = metricsBag.Property3;
            }
        }

        internal static MetricTelemetry CreateDerivedMetric(this MetricTelemetry metricTelemetry, string nameSuffix, double value)
        {
            var derivedTelemetry = new MetricTelemetry(string.Format(CultureInfo.InvariantCulture, "{0}_{1}", metricTelemetry.Name, nameSuffix), value)
            {
                Timestamp = metricTelemetry.Timestamp
            };

            derivedTelemetry.Context.GetInternalContext().SdkVersion = metricTelemetry.Context.GetInternalContext().SdkVersion;

            return derivedTelemetry;
        }
    }
}