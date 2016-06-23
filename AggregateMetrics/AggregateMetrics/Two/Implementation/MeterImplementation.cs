namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Threading;

    class MeterImplementation : NamedCounterValueBase, IMeter, ICounterValue
    {
        private long compositeValue;

        public MeterImplementation(string name, TelemetryContext context)
            : base(name, context)
        {
        }

        public DataContracts.MetricTelemetry Value
        {
            get 
            {
                var metric = this.GetInitializedMetricTelemetry();

                var curValue = this.compositeValue;
                var count = (int)(curValue & ((1 << 24) - 1));
                double value = curValue >> 24;

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
            long curValue = Interlocked.Exchange(ref this.compositeValue, 0);

            var count = (int)(curValue & ((1 << 24) - 1));
            double value = curValue >> 24;

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

        public void Mark()
        {
            this.Mark(1);
        }

        public void Mark(int markCount)
        {
            long delta = ((markCount) << 24) + 1;
            Interlocked.Add(ref this.compositeValue, delta);
        }
    }
}
