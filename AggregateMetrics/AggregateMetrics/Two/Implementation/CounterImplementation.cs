namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;
    using System.Threading;
    using DataContracts;

    internal class CounterImplementation : NamedCounterValueBase, ICounter, ICounterValue
    {
        private int value;

        public CounterImplementation(string name, TelemetryContext context)
            : base(name, context)
        {
        }

        public void Increment()
        {
            Interlocked.Increment(ref this.value);
        }

        public void Decrement()
        {
            Interlocked.Decrement(ref this.value);
        }

        public MetricTelemetry GetValueAndReset()
        {
            var metric = this.GetInitializedMetricTelemetry();
            metric.Value = this.value;
            return metric;
        }

        public void Increment(int count)
        {
            Interlocked.Add(ref this.value, count);
        }

        public void Decrement(int count)
        {
            Interlocked.Add(ref this.value, -count);
        }
    }
}
