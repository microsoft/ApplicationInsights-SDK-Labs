using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    internal class DefaultCounters
    {
        private static readonly DefaultCounters instance = new DefaultCounters();

        private DefaultCounters() { this.Initialize(); }

        /// <summary>
        /// Return the only instance of DefaultCounters.
        /// </summary>
        public static DefaultCounters Instance
        {
            get
            {
                return instance;
            }
        }

        Dictionary<FlexiblePerformanceCounterGauge, MetricTelemetry> defaultCounters;
        Dictionary<FlexiblePerformanceCounterGauge, RateCounter> timeDependentCounters;

        /// <summary>
        /// Initializes the dictionaries for the default performance counters.
        /// </summary>
        public void Initialize()
        {
            defaultCounters = new Dictionary<FlexiblePerformanceCounterGauge, MetricTelemetry>()
            {
                { new FlexiblePerformanceCounterGauge("kernelTime"), new MetricTelemetry() },
                { new FlexiblePerformanceCounterGauge("userTime"), new MetricTelemetry() },
                { new FlexiblePerformanceCounterGauge("appRequestExecTime"), new MetricTelemetry() },
                { new FlexiblePerformanceCounterGauge("privateBytes"), new MetricTelemetry() },
                { new FlexiblePerformanceCounterGauge("requestsInApplicationQueue"), new MetricTelemetry() }
            };

            timeDependentCounters = new Dictionary<FlexiblePerformanceCounterGauge, RateCounter>()
            {
                { new FlexiblePerformanceCounterGauge("requestsTotal"), new RateCounter() },
                { new FlexiblePerformanceCounterGauge("exceptionsThrown"), new RateCounter() },
                { new FlexiblePerformanceCounterGauge("readIoBytes"), new RateCounter() },
                { new FlexiblePerformanceCounterGauge("writeIoBytes"), new RateCounter() },
                { new FlexiblePerformanceCounterGauge("otherIoBytes"), new RateCounter() }
            };
        }

        /// <summary>
        /// Gets the default performance counters.
        /// </summary>
        /// <returns> List of MetricTelemetry objects</returns>
        private List<MetricTelemetry> GetCounters(bool useEnvironmentVariables = true)
        {
            List<MetricTelemetry> metrics = new List<MetricTelemetry>();

            foreach (var item in defaultCounters.ToList())
            {
                if (useEnvironmentVariables)
                    defaultCounters[item.Key] = item.Key.GetValueAndReset();
                else
                {
                    MetricTelemetry metric = new MetricTelemetry();
                    metric.Value = defaultCounters[item.Key].Value;
                }

                metrics.Add(defaultCounters[item.Key]);
            }

            foreach (var item in timeDependentCounters.ToList())
            {
                bool firstValue = timeDependentCounters[item.Key].DateTime == System.DateTime.MinValue;

                RateCounter counter = new RateCounter();
                counter.MetricTelemetry = item.Key.GetValueAndReset();
                counter.DateTime = System.DateTime.Now;

                if (!firstValue)
                {
                    MetricTelemetry metric = counter.MetricTelemetry;
                    metric.Value = (counter.MetricTelemetry.Value - timeDependentCounters[item.Key].MetricTelemetry.Value) / (counter.DateTime.Second - timeDependentCounters[item.Key].DateTime.Second);
                    metrics.Add(metric);
                }

                timeDependentCounters[item.Key] = counter;
            }

            return metrics;
        }
    }
}