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
        /// Gets a counter.
        /// </summary>
        /// <param name="counterName">Name of the counter to retrieve.</param>
        /// <returns>The counter identified by counterName</returns>
        public ICounterValue GetCounter(string counterName)
        {
            switch (counterName)
            {
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Execution Time":
                    return new PerformanceCounterFromJsonGauge(
                        counterName,
                        "appRequestExecTime");
                case @"\Process(??APP_WIN32_PROC??)\Private Bytes":
                    return new PerformanceCounterFromJsonGauge(
                        counterName,
                        "privateBytes");
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests In Application Queue":
                    return new PerformanceCounterFromJsonGauge(
                        counterName,
                        "requestsInApplicationQueue");
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests/Sec":
                    return new RateCounterGauge(
                        counterName, 
                        "requestsTotal");
                case @"\.NET CLR Exceptions(??APP_CLR_PROC??)\# of Exceps Thrown / sec":
                    return new RateCounterGauge(
                        counterName,
                        "exceptionsThrown");
                case @"\Processor(_Total)\% Processor Time":
                    return new SumUpGauge(
                        counterName,
                        new PerformanceCounterFromJsonGauge("kernelTime", "kernelTime"),
                        new PerformanceCounterFromJsonGauge("userTime", "userTime"));
                case @"\Process(??APP_WIN32_PROC??)\IO Data Bytes/sec":
                    return new RateCounterGauge(
                        counterName, 
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
                        counterName,
                        "handles");
                case @"\Process(??APP_WIN32_PROC??)\Thread Count":
                    return new PerformanceCounterFromJsonGauge(
                        counterName,
                        "threads");
                default:
                    throw new ArgumentException("Performance counter was not found.", counterName);
            }
        }
    }
}
