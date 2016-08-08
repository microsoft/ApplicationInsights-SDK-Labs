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

        private ICacheHelper cacheHelper;

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
        internal RateCounterGauge(string name, ICacheHelper cache) 
        {
            this.name = name;
            this.cacheHelper = cache;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCounterGauge"/> class.
        /// </summary>
        /// <param name="name"> Name of counter variable.</param>
        public RateCounterGauge(string name)
            : this(name, CacheHelper.Instance)
        {
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
                this.lastValue = cacheHelper.GetCounterValue(this.name);
                this.dateTime = currentTime;

                return metric;
            }

            var timeDifferenceInSeconds = currentTime.Subtract(this.dateTime).Seconds;

            metric.Value = (timeDifferenceInSeconds != 0) ? (cacheHelper.GetCounterValue(this.name) - (double)this.lastValue) / timeDifferenceInSeconds : 0;
            this.lastValue = cacheHelper.GetCounterValue(this.name);
            this.dateTime = currentTime;

            return metric;
        }
    }
}
