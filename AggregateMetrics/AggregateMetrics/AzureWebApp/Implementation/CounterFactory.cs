namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp.Implementation
{
    using System;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;

    /// <summary>
    /// Factory to create different counters.
    /// </summary>
    internal class CounterFactory
    {
        /// <summary>
        /// Set metrics alias to be the value given by the user.
        /// </summary>
        /// <param name="counterName">Name of the counter to retrieve.</param>
        /// <param name="reportAs">Alias to report the counter.</param>
        /// <returns>Alias that will be used for the counter.</returns>
        private string SetCounterName(string counterName, string reportAs)
        {
            if (reportAs == null)
                return counterName;
            else
                return reportAs;
        }

        /// <summary>
        /// Gets a counter.
        /// </summary>
        /// <param name="counterName">Name of the counter to retrieve.</param>
        /// <param name="reportAs">Alias to report the counter under.</param>
        /// <returns>The counter identified by counterName</returns>
        public ICounterValue GetCounter(string counterName, string reportAs = null)
        {
            switch (counterName)
            {
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Execution Time":
                    return new PerformanceCounterFromJsonGauge(
                        SetCounterName(counterName, reportAs),
                        "appRequestExecTime");
                case @"\Process(??APP_WIN32_PROC??)\Private Bytes":
                    return new PerformanceCounterFromJsonGauge(
                        SetCounterName(counterName, reportAs),
                        "privateBytes");
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests In Application Queue":
                    return new PerformanceCounterFromJsonGauge(
                        SetCounterName(counterName, reportAs),
                        "requestsInApplicationQueue");
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests/Sec":
                    return new RateCounterGauge(
                        SetCounterName(counterName, reportAs), 
                        "requestsTotal");
                case @"\.NET CLR Exceptions(??APP_CLR_PROC??)\# of Exceps Thrown / sec":
                    return new RateCounterGauge(
                        SetCounterName(counterName, reportAs),
                        "exceptionsThrown");
                case @"\Processor(_Total)\% Processor Time":
                    return new SumUpGauge(
                        SetCounterName(counterName, reportAs),
                        new PerformanceCounterFromJsonGauge("kernelTime", "kernelTime"),
                        new PerformanceCounterFromJsonGauge("userTime", "userTime"));
                case @"\Process(??APP_WIN32_PROC??)\IO Data Bytes/sec":
                    return new RateCounterGauge(
                        SetCounterName(counterName, reportAs), 
                        "ioDataBytesRate",
                        new SumUpGauge(
                            "ioDataBytesRate",
                            new PerformanceCounterFromJsonGauge(
                                "readIoBytes", 
                                "readIoBytes"),
                            new PerformanceCounterFromJsonGauge(
                                "writeIoBytes", 
                                "writeIoBytes"),
                            new PerformanceCounterFromJsonGauge(
                                "otherIoBytes", 
                                "otherIoBytes")));
                case @"\Process(??APP_WIN32_PROC??)\Handle Count":
                    return new PerformanceCounterFromJsonGauge(
                        SetCounterName(counterName, reportAs),
                        "handles");
                case @"\Process(??APP_WIN32_PROC??)\Thread Count":
                    return new PerformanceCounterFromJsonGauge(
                        SetCounterName(counterName, reportAs),
                        "threads");
                default:
                    throw new ArgumentException("Performance counter was not found.", counterName);
            }
        }
    }
}
