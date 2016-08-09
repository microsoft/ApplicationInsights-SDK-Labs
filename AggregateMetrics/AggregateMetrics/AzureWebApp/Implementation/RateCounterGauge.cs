namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;

    /// <summary>
    /// Struct for metrics dependant on time.
    /// </summary>
    internal class RateCounterGauge : ICounterValue
    {
        /// <summary>
        /// Name of the counter.
        /// </summary>
        private string name;

        /// <summary>
        /// Json identifier of the counter variable.
        /// </summary>
        private string jsonId;

        private ICounterValue counter;

        private double? lastValue;

        private ICachedEnvironmentVariableAccess cacheHelper;

        /// <summary>
        /// DateTime object to keep track of the last time this metric was retrieved.
        /// </summary>
        private DateTimeOffset dateTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCounterGauge"/> class.
        /// </summary>
        /// <param name="name"> Name of counter variable.</param>
        /// <param name="jsonId">Json identifier of the counter variable.</param>
        /// <param name="counter">Dependant counter.</param>
        public RateCounterGauge(string name, string jsonId, ICounterValue counter = null)
            : this(name, jsonId, counter, CacheHelper.GetInstance)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RateCounterGauge"/> class. 
        /// This constructor is intended for Unit Tests.
        /// </summary>
        /// <param name="name"> Name of the counter variable.</param>
        /// /// <param name="jsonId">Json identifier of the counter variable.</param>
        /// <param name="counter">Dependant counter.</param>
        /// <param name="cache"> Cache object.</param>
        internal RateCounterGauge(string name, string jsonId, ICounterValue counter, ICachedEnvironmentVariableAccess cache)
        {
            this.name = name;
            this.jsonId = jsonId;
            this.counter = counter;
            this.cacheHelper = cache;
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
                this.lastValue = this.cacheHelper.GetCounterValue(this.jsonId);
                this.dateTime = currentTime;

                return metric;
            }

            var timeDifferenceInSeconds = currentTime.Subtract(this.dateTime).Seconds;

            metric.Value = (timeDifferenceInSeconds != 0) ? (this.cacheHelper.GetCounterValue(this.jsonId) - (double)this.lastValue) / timeDifferenceInSeconds : 0;
            this.lastValue = this.cacheHelper.GetCounterValue(this.jsonId);
            this.dateTime = currentTime;

            return metric;
        }
    }
}
