namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;

    /// <summary>
    /// Gauge that sums up the values of different gauges.
    /// </summary>
    public class SumUpGauge : ICounterValue
    {
        private string name;

        private List<FlexiblePerformanceCounterGauge> gaugesToSum;

        /// <summary>
        /// Initializes a new instance of the <see cref="SumUpGauge"/> class.
        /// </summary>
        /// <param name="name"> Name of the SumUpGauge.</param>
        /// <param name="gauges"> Gauges to sum.</param>
        public SumUpGauge(string name, params FlexiblePerformanceCounterGauge[] gauges)
        {
            this.name = name;
            this.gaugesToSum = new List<FlexiblePerformanceCounterGauge>();

            foreach (FlexiblePerformanceCounterGauge gauge in gauges)
            {
                this.gaugesToSum.Add(gauge);
            }
        }

        /// <summary>
        /// Returns the current value of the sum of all different gauges attached to this one and resets their values.
        /// </summary>
        /// <returns> MetricTelemetry object</returns>
        public MetricTelemetry GetValueAndReset()
        {
            var metric = new MetricTelemetry();

            metric.Name = this.name;
            metric.Value = 0;

            foreach (FlexiblePerformanceCounterGauge gauge in this.gaugesToSum)
            {
                metric.Value += gauge.GetValueAndReset().Value;
            }

            return metric;
        }
    }
}
