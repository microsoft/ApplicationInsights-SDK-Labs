namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;
    using System.Threading;
    using DataContracts;

    internal class CounterImplementation : NamedCounterValueBase, ICounter, ICounterValue
    {
        private long value;

        public CounterImplementation(string name, TelemetryContext context)
            : base(name, context)
        {
        }

        public MetricTelemetry Value
        {
            get
            {
                var metric = this.GetInitializedMetricTelemetry();
                metric.Value = this.value;
                return metric;
            }
        }

        public void Increment()
        {
            Interlocked.Increment(ref value);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref value);
        }

        public MetricTelemetry GetValueAndReset()
        {
            return this.Value;
        }

        public void Increment(long value)
        {
            Interlocked.Add(ref value, value);
        }

        public void Decrement(long value)
        {
            Interlocked.Add(ref value, -1 * value);
        }
    }
}
