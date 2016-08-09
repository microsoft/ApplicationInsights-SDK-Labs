namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp.Implementation
{
    using System.ComponentModel;
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
                        @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Execution Time",
                        "appRequestExecTime");
                case @"\Process(??APP_WIN32_PROC??)\Private Bytes":
                    return new PerformanceCounterFromJsonGauge(
                        @"\Process(??APP_WIN32_PROC??)\Private Bytes",
                        "privateBytes");
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests In Application Queue":
                    return new PerformanceCounterFromJsonGauge(
                        @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests In Application Queue",
                        "requestsInApplicationQueue");
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests/Sec":
                    return new RateCounterGauge(
                        @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests/Sec", 
                        "requestsTotal");
                case @"\.NET CLR Exceptions(??APP_CLR_PROC??)\# of Exceps Thrown / sec":
                    return new RateCounterGauge(
                        @"\.NET CLR Exceptions(??APP_CLR_PROC??)\# of Exceps Thrown / sec",
                        "exceptionsThrown");
                case @"\Processor(_Total)\% Processor Time":
                    return new SumUpGauge(
                        @"\Processor(_Total)\% Processor Time",
                        new PerformanceCounterFromJsonGauge("kernelTime", "kernelTime"),
                        new PerformanceCounterFromJsonGauge("userTime", "userTime"));
                case @"\Process(??APP_WIN32_PROC??)\IO Data Bytes/sec":
                    return new RateCounterGauge(
                        @"\Process(??APP_WIN32_PROC??)\IO Data Bytes/sec", 
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
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}
