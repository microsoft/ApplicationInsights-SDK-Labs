namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Class that gives the user the default performance counters.
    /// </summary>
    internal class DefaultCounters
    {
        private static readonly DefaultCounters DefaultCountersInstance = new DefaultCounters();

        Dictionary<FlexiblePerformanceCounterGauge, MetricTelemetry> defaultCounters;
        Dictionary<FlexiblePerformanceCounterGauge, RateCounter> rateDefaultCounters;

        SumUpGauge processorTime = new SumUpGauge(
                "processorTime",
                new FlexiblePerformanceCounterGauge("kernelTime"),
                new FlexiblePerformanceCounterGauge("userTime"));

        SumUpGauge ioDataBytesRate = new SumUpGauge(
                "ioDataBytesRate",
                new FlexiblePerformanceCounterGauge("readIoBytes"),
                new FlexiblePerformanceCounterGauge("writeIoBytes"),
                new FlexiblePerformanceCounterGauge("otherIoBytes"));

        private DefaultCounters()
        {
            this.Initialize();
        }

        /// <summary>
        /// Gets the only instance of DefaultCounters.
        /// </summary>
        public static DefaultCounters Instance
        {
            get
            {
                return DefaultCountersInstance;
            }
        }

        /// <summary>
        /// Initializes the dictionaries for the default performance counters.
        /// </summary>
        public void Initialize()
        {
            this.defaultCounters = new Dictionary<FlexiblePerformanceCounterGauge, MetricTelemetry>()
            {
                { new FlexiblePerformanceCounterGauge("appRequestExecTime"), new MetricTelemetry() },
                { new FlexiblePerformanceCounterGauge("privateBytes"), new MetricTelemetry() },
                { new FlexiblePerformanceCounterGauge("requestsInApplicationQueue"), new MetricTelemetry() }
            };

            this.rateDefaultCounters = new Dictionary<FlexiblePerformanceCounterGauge, RateCounter>()
            {
                { new FlexiblePerformanceCounterGauge("requestsTotal"), new RateCounter() },
                { new FlexiblePerformanceCounterGauge("exceptionsThrown"), new RateCounter() }
            };
        }

        /// <summary>
        /// Gets the default performance counters.
        /// </summary>
        /// <returns> List of MetricTelemetry objects</returns>
        private List<MetricTelemetry> GetCounters()
        {
            List<MetricTelemetry> metrics = new List<MetricTelemetry>();

            foreach (var item in this.defaultCounters.ToList())
            {
                this.defaultCounters[item.Key] = item.Key.GetValueAndReset();
                metrics.Add(this.defaultCounters[item.Key]);
            }

            foreach (var item in this.rateDefaultCounters.ToList())
            {
                bool firstValue = this.rateDefaultCounters[item.Key].DateTime == System.DateTime.MinValue;

                RateCounter counter = new RateCounter();
                counter.MetricTelemetry = item.Key.GetValueAndReset();
                counter.DateTime = System.DateTime.Now;

                if (!firstValue)
                {
                    MetricTelemetry metric = counter.MetricTelemetry;
                    metric.Value = (counter.MetricTelemetry.Value - this.rateDefaultCounters[item.Key].MetricTelemetry.Value) / (counter.DateTime.Second - this.rateDefaultCounters[item.Key].DateTime.Second);
                    metrics.Add(metric);
                }

                this.rateDefaultCounters[item.Key] = counter;
            }

            return metrics;
        }
    }
}