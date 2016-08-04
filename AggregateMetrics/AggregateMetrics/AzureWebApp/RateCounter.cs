namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;

    /// <summary>
    /// Struct for metrics dependant on time.
    /// </summary>
    public class RateCounterGauge : ICounterValue
    {
        private string name;

        private double? lastValue;

        private dynamic cacheHelper;

        /// <summary>
        /// DateTime object to keep track of the last time this metric was retrieved.
        /// </summary>
        private DateTimeOffset dateTime;

        /// <summary>
        /// Initializes a new instance of <see cref="RateCounterGauge"/> class. 
        /// This constructor is intended for Unit Tests.
        /// </summary>
        /// <param name="name"> Name of the counter variable.</param>
        /// <param name="cache"> Cache object.</param>
        internal RateCounterGauge(string name, dynamic cache)
        {
            this.name = name;
            this.cacheHelper = cache;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCounterGauge"/> class.
        /// </summary>
        /// <param name="name"> Name of counter variable.</param>
        public RateCounterGauge(string name)
        {
            this.name = name;
            this.cacheHelper = CacheHelper.Instance;
        }

        /// <summary>
        /// Returns the current value of the rate counter if enough information exists.
        /// </summary>
        /// <returns> MetricTelemetry object.</returns>
        public MetricTelemetry GetValueAndReset()
        {
            MetricTelemetry metric = new MetricTelemetry();

            metric.Name = this.name;
            DateTimeOffset currentTime = System.DateTimeOffset.Now;

            if (this.lastValue == null)
            {
                this.lastValue = cacheHelper;
                this.dateTime = currentTime;

                return metric;
            }

            metric.Value = ((double)this.lastValue - cacheHelper) / (currentTime.Second - this.dateTime.Second);
            this.lastValue = cacheHelper;
            this.dateTime = currentTime;

            return metric;
        }
    }
}
