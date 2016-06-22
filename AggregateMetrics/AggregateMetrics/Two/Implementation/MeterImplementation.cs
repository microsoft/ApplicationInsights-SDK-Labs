namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Threading;

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
                if (count != 0)
                {
                    metric.Value = value / count;
                    metric.Count = count;
                }
                else
                {
                    metric.Value = 0;
                    metric.Count = 0;
                }
                return metric;
            }
        }

        public DataContracts.MetricTelemetry GetValueAndReset()
        {
            var returnValue = Interlocked.Exchange(ref value, 0);
            var returnCount = Interlocked.Exchange(ref count, 0);

            var metric = this.GetInitializedMetricTelemetry();
            if (returnValue != 0)
            {
                metric.Value = returnValue / returnCount;
                metric.Count = returnCount;
            }
            else
            {
                metric.Value = 0;
                metric.Count = 0;
            }
            return metric;
        }

        public void Mark()
        {
            //TODO: potentially we need to increment them together in one atomic operation
            Interlocked.Increment(ref value);
            Interlocked.Increment(ref count);
        }

        public void Mark(int markCount)
        {
            Interlocked.Add(ref this.value, markCount);
            Interlocked.Increment(ref this.count);
        }
    }
}
