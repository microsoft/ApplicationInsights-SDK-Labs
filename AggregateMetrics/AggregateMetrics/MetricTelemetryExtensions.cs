namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics
{
    using System.Globalization;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.Implementation; 

    internal static class MetricTelemetryExtensions
    {
        internal static void AddProperties(this MetricTelemetry metricTelemetry, MetricsBag metricsBag, string p1Name, string p2Name, string p3Name)
        {
            if (metricsBag.Property1 != null)
            {
                metricTelemetry.Properties[p1Name] = metricsBag.Property1;
            }

            if (metricsBag.Property2 != null)
            {
                metricTelemetry.Properties[p2Name] = metricsBag.Property2;
            }

            if (metricsBag.Property3 != null)
            {
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