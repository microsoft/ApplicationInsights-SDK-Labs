namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;

    /// <summary>
    /// Struct for metrics dependant on time.
    /// </summary>
    public class RateCounter : ICounterValue
    {
        private string name;

        private double? lastValue;

        /// <summary>
        /// DateTime object to keep track of the last time this metric was retrieved.
        /// </summary>
        private DateTime dateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCounter"/> class.
        /// </summary>
        /// <param name="name"> Name of counter variable.</param>
        public RateCounter(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Returns the current value of the rate counter if enough information exists.
        /// </summary>
        /// <returns> MetricTelemetry object.</returns>
        public MetricTelemetry GetValueAndReset()
        {
            MetricTelemetry metric = new MetricTelemetry();

            metric.Name = this.name;

            if (this.lastValue == null)
            {
                this.lastValue = CacheHelper.Instance.GetCounterValue(this.name);
                this.dateTime = System.DateTime.Now;

                return metric;
            }

            metric.Value = ((double)this.lastValue - CacheHelper.Instance.GetCounterValue(this.name)) / (System.DateTime.Now.Second - this.dateTime.Second);
            this.lastValue = CacheHelper.Instance.GetCounterValue(this.name);
            this.dateTime = System.DateTime.Now;

            return metric;
        }
    }
}
