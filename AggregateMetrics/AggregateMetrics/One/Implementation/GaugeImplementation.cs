using System;
using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
{
    internal class GaugeImplementation : ICounterValue
    {
        private readonly Func<long> valueFunc;

        public GaugeImplementation(Func<long> valueFunc)
        {
            this.valueFunc = valueFunc;
        }

        public MetricTelemetry Value
        {
            get
            {
                var metric = new MetricTelemetry();
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