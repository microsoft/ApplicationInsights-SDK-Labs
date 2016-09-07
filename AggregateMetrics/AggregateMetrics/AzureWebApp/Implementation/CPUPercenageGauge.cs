namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using System;

    /// <summary>
    /// Gauge that sums up the values of different gauges.
    /// </summary>
    internal class CPUPercenageGauge : ICounterValue
    {
        /// <summary>
        /// Name of the counter.
        /// </summary>
        private string name;

        private MetricTelemetry lastCollectedValue;

        private DateTimeOffset lastCollectedTime;

        private ICounterValue valueProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SumUpGauge"/> class.
        /// </summary>
        /// <param name="name"> Name of the SumUpGauge.</param>
        /// <param name="value"> Gauges to sum.</param>
        public CPUPercenageGauge(string name, ICounterValue value)
        {
            this.name = name;
            this.valueProvider = value;
        }

        /// <summary>
        /// Returns the current value of the sum of all different gauges attached to this one and resets their values.
        /// </summary>
        /// <returns> MetricTelemetry object</returns>
        public MetricTelemetry GetValueAndReset()
        {
            var previouslyCollectedValue = this.lastCollectedValue;
            this.lastCollectedValue = this.valueProvider.GetValueAndReset();

            var previouslyCollectedTime = this.lastCollectedTime;
            this.lastCollectedTime = DateTimeOffset.UtcNow;

            double value = 0;
            if (previouslyCollectedTime != DateTimeOffset.MinValue)
            {
                var baseValue = lastCollectedTime.Ticks - previouslyCollectedTime.Ticks;
                baseValue = baseValue != 0 ? baseValue : 1;

                value = (this.lastCollectedValue.Value - previouslyCollectedValue.Value) / baseValue * 100.0;
            }

            var result = new MetricTelemetry();
            result.Name = this.name;
            result.Value = value;

            return result;
        }
    }
}
