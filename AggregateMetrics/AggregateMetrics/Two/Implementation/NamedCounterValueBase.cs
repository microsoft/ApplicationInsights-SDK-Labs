using Microsoft.ApplicationInsights.DataContracts;
namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    internal class NamedCounterValueBase
    {
        private readonly string name;
        private readonly TelemetryContext context;

        public NamedCounterValueBase(string name, TelemetryContext context)
        {
            this.name = name;
            //TODO: context needs to be copied, not used by reference
            this.context = context;
        }

        public MetricTelemetry GetInitializedMetricTelemetry()
        {
            var metric = new MetricTelemetry();
            metric.Name = this.name;

            //TODO: copy all the rest of the context
            foreach (var prop in this.context.Properties)
            {
                metric.Context.Properties.Add(prop);
            }

            return metric;
        }
    }
}
