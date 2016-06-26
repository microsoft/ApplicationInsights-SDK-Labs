namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;
    using System.Threading;
    using Microsoft.ApplicationInsights.DataContracts;

    internal class HistogramImplementation : NamedCounterValueBase, ICounterValue, IHistogram
    {
        private long compositeValue;

        public HistogramImplementation(string name, TelemetryContext context)
            : base(name, context)
        {
        }

        public MetricTelemetry Value
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

        public MetricTelemetry GetValueAndReset()
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

        public void Update(int value)
        {
            long delta = ((value) << 24) + 1;
            Interlocked.Add(ref this.compositeValue, delta);
        }
    }
}
