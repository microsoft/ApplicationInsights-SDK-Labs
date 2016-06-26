namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Diagnostics;
    using System.Threading;

    class MeterImplementation : NamedCounterValueBase, IMeter, ICounterValue
    {
        private int value;
        private Stopwatch timer;

        public MeterImplementation(string name, TelemetryContext context)
            : base(name, context)
        {
            value = 0;
            timer = Stopwatch.StartNew();
        }

        public DataContracts.MetricTelemetry Value
        {
            get 
            {
                var metric = this.GetInitializedMetricTelemetry();

                var currentValue = this.value;

                var seconds = this.timer.Elapsed.TotalSeconds;
                if (seconds == 0)
                {
                    seconds = 1;
                }

                metric.Value = currentValue / seconds;

                return metric;
            }
        }

        public DataContracts.MetricTelemetry GetValueAndReset()
        {
            var currentValue = Interlocked.Exchange(ref this.value, 0);

            var seconds = this.timer.Elapsed.TotalSeconds;
            if (seconds == 0)
            {
                seconds = 1;
            }

            this.timer.Restart();
            this.value = 0;

            var metric = this.GetInitializedMetricTelemetry();

            metric.Value = currentValue / seconds;

            return metric;
        }

        public void Mark()
        {
            Interlocked.Increment(ref this.value);
        }

        public void Mark(int count)
        {
            Interlocked.Add(ref this.value, count);
        }
    }
}
