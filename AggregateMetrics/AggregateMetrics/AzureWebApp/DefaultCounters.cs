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
        Dictionary<FlexiblePerformanceCounterGauge, TimeDependentCounter> timeDependentCounters;

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

            timeDependentCounters = new Dictionary<FlexiblePerformanceCounterGauge, TimeDependentCounter>()
            {
                { new FlexiblePerformanceCounterGauge("requestsTotal"), new TimeDependentCounter() },
                { new FlexiblePerformanceCounterGauge("exceptionsThrown"), new TimeDependentCounter() },
                { new FlexiblePerformanceCounterGauge("readIoBytes"), new TimeDependentCounter() },
                { new FlexiblePerformanceCounterGauge("writeIoBytes"), new TimeDependentCounter() },
                { new FlexiblePerformanceCounterGauge("otherIoBytes"), new TimeDependentCounter() }
            };
        }

        /// <summary>
        /// Gets the default performance counters.
        /// </summary>
        /// <returns> List of MetricTelemetry objects</returns>
        public List<MetricTelemetry> GetCounters()
        {
            List<MetricTelemetry> metrics = new List<MetricTelemetry>();

            foreach (var item in defaultCounters.ToList())
            {
                defaultCounters[item.Key] = item.Key.GetValueAndReset();
                metrics.Add(defaultCounters[item.Key]);
            }

            foreach (var item in timeDependentCounters.ToList())
            {
                bool firstValue = timeDependentCounters[item.Key].DateTime == System.DateTime.MinValue;

                TimeDependentCounter counter = new TimeDependentCounter();
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

        /// <summary>
        /// Gets the default performance counters. This method is meant to be used only for testing.
        /// </summary>
        /// <returns> List of MetricTelemetry objects</returns>
        public List<MetricTelemetry> GetCountersHttp()
        {
            List<MetricTelemetry> metrics = new List<MetricTelemetry>();

            foreach (var item in defaultCounters.ToList())
            {
                defaultCounters[item.Key] = item.Key.GetValueAndResetHttp();
                metrics.Add(defaultCounters[item.Key]);
            }

            foreach (var item in timeDependentCounters.ToList())
            {
                bool firstValue = timeDependentCounters[item.Key].DateTime == System.DateTime.MinValue;

                TimeDependentCounter counter = new TimeDependentCounter();
                counter.MetricTelemetry = item.Key.GetValueAndResetHttp();
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