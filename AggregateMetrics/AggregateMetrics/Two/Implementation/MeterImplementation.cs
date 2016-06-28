namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Diagnostics;
    using System.Threading;

    internal class MeterImplementation : NamedCounterValueBase, IMeter, ICounterValue
    {
        private int value;
        private Stopwatch timer;

        private bool shouldCalculateRate;
        private bool shouldCalculateSum;

        public MeterImplementation(string name, TelemetryContext context, MeterAggregations aggregations)
            : base(name, context)
        {
            this.value = 0;
            this.shouldCalculateRate = (aggregations & MeterAggregations.Rate) == MeterAggregations.Rate;
            this.shouldCalculateSum = (aggregations & MeterAggregations.Sum) == MeterAggregations.Sum;

            if (this.shouldCalculateRate)
            {
                timer = Stopwatch.StartNew();
            }
        }

        public DataContracts.MetricTelemetry Value
        {
            get 
            {
                var metric = this.GetInitializedMetricTelemetry();

                var currentValue = this.value;

                if (this.shouldCalculateRate)
                {
                    var seconds = this.timer.Elapsed.TotalSeconds;
                    if (seconds == 0)
                    {
                        seconds = 1;
                    }

                    metric.Value = currentValue / seconds;
                }
                else
                {
                    metric.Value = currentValue;
                }

                return metric;
            }
        }

        public DataContracts.MetricTelemetry GetValueAndReset()
        {
            var currentValue = Interlocked.Exchange(ref this.value, 0);
            var metric = this.GetInitializedMetricTelemetry();

            if (this.shouldCalculateRate)
            {
                var seconds = this.timer.Elapsed.TotalSeconds;
                if (seconds == 0)
                {
                    seconds = 1;
                }

                this.timer.Restart();
                metric.Value = currentValue / seconds;
            }
            else
            {
                metric.Value = currentValue;
            }

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
