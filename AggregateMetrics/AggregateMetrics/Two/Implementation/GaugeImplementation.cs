using System;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    internal class GaugeImplementation : NamedCounterValueBase, ICounterValue
    {
        private readonly Func<long> valueFunc;

        public GaugeImplementation(string name, TelemetryContext context, Func<long> valueFunc)
            : base(name, context)
        {
            this.valueFunc = valueFunc;
        }

        public MetricTelemetry Value
        {
            get
            {
                var metric = this.GetInitializedMetricTelemetry();
                //TODO: Add try/catch here
                metric.Value = valueFunc();
                return metric;
            }
        }

        public MetricTelemetry GetValueAndReset()
        {
            return this.Value;
        }
    }
}