namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.ApplicationInsights.DataContracts;
    using Two;

    /// <summary>
    /// Class that gives the user the default performance counters.
    /// </summary>
    internal class DefaultCounters
    {
        List<ICounterValue> defaultCounters;

        /// <summary>
        /// Initializes the dictionaries for the default performance counters.
        /// </summary>
        public void Initialize()
        {
            this.defaultCounters = new List<ICounterValue>
            {
                new FlexiblePerformanceCounterGauge("appRequestExecTime"),
                new FlexiblePerformanceCounterGauge("privateBytes"),
                new FlexiblePerformanceCounterGauge("requestsInApplicationQueue"),
                new RateCounterGauge("requestsTotal"),
                new RateCounterGauge("exceptionsThrown"),
                new SumUpGauge("processorTime", 
                    new FlexiblePerformanceCounterGauge("kernelTime"), 
                    new FlexiblePerformanceCounterGauge("userTime")),
                new SumUpGauge("ioDataBytesRate",
                    new FlexiblePerformanceCounterGauge("readIoBytes"), 
                    new FlexiblePerformanceCounterGauge("writeIoBytes"), 
                    new FlexiblePerformanceCounterGauge("otherIoBytes"))
            };
        }
    }
}