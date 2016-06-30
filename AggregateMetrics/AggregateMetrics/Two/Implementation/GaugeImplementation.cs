namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;
    using Microsoft.ApplicationInsights.DataContracts;

    internal class GaugeImplementation : NamedCounterValueBase, ICounterValue
    {
        private readonly Func<int> valueFunc;

        public GaugeImplementation(string name, TelemetryContext context, Func<int> valueFunc)
            : base(name, context)
        {
            this.valueFunc = valueFunc;
        }

        public MetricTelemetry GetValueAndReset()
        {
            var metric = this.GetInitializedMetricTelemetry();
            try
            {
                metric.Value = valueFunc();
            }
            catch (Exception)
            {
                //TODO: trace the error
            }
            return metric;
        }
    }
}