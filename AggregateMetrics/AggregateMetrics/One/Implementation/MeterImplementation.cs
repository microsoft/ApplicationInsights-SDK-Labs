namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
{
    using Microsoft.ApplicationInsights.DataContracts;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class MeterImplementation : NamedCounterValueBase, IMeter, ICounterValue
    {
        private int value;

        private int count;

        public MeterImplementation(string name, TelemetryContext context)
            : base(name, context)
        {
        }

        public DataContracts.MetricTelemetry Value
        {
            get 
            {
                var metric = this.GetInitializedMetricTelemetry();
                metric.Value = value / count;
                metric.Count = count;
                return metric;
            }
        }

        public DataContracts.MetricTelemetry GetValueAndReset()
        {
            var returnValue = Interlocked.Exchange(ref value, 0);
            var returnCount = Interlocked.Exchange(ref count, 0);

            var metric = this.GetInitializedMetricTelemetry();
            metric.Value = returnValue / returnCount;
            metric.Count = returnCount;
            return metric;
        }

        public void Mark()
        {
            //TODO: potentially we need to increment them together in one atomic operation
            Interlocked.Increment(ref value);
            Interlocked.Increment(ref count);
        }

        public void Mark(int count)
        {
            Interlocked.Add(ref value, count);
            Interlocked.Increment(ref count);
        }
    }
}
