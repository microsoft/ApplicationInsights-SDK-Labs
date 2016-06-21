namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
{
    using System;
    using System.Threading;
    using DataContracts;

    internal class CounterImplementation : ICounter, ICounterValue
    {
        private long value;

        public MetricTelemetry Value
        {
            get
            {
                var metric = new MetricTelemetry();
                metric.Value = this.value;
                metric.Count = 1;
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
            var returnValue = Interlocked.Exchange(ref value, 0);

            var metric = new MetricTelemetry();
            metric.Value = returnValue;
            metric.Count = 1;
            return metric;
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
