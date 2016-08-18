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
        /// <param name="reportAs">Alias to report the counter under.</param>
        /// <returns>The counter identified by counterName</returns>
        public ICounterValue GetCounter(string counterName, string reportAs)
        {
            switch (counterName)
            {
                // Default performance counters
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Execution Time":
                    return new PerformanceCounterFromJsonGauge(
                        reportAs,
                        "appRequestExecTime",
                        AzureWebApEnvironmentVariables.AspNet);
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests In Application Queue":
                    return new PerformanceCounterFromJsonGauge(
                        reportAs,
                        "requestsInApplicationQueue",
                        AzureWebApEnvironmentVariables.AspNet);
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests/Sec":
                    return new RateCounterGauge(
                        reportAs,
                        "requestsTotal",
                        AzureWebApEnvironmentVariables.All);
                case @"\.NET CLR Exceptions(??APP_CLR_PROC??)\# of Exceps Thrown / sec":
                    return new RateCounterGauge(
                        reportAs,
                        "exceptionsThrown",
                        AzureWebApEnvironmentVariables.CLR);
                case @"\Process(??APP_WIN32_PROC??)\Private Bytes":
                    return new PerformanceCounterFromJsonGauge(
                    reportAs,
                    "privateBytes",
                    AzureWebApEnvironmentVariables.App);
                case @"\Process(?? APP_WIN32_PROC ??)\% Processor Time":
                    return new SumUpGauge(
                        reportAs,
                        new PerformanceCounterFromJsonGauge("kernelTime", "kernelTime", AzureWebApEnvironmentVariables.App),
                        new PerformanceCounterFromJsonGauge("userTime", "userTime", AzureWebApEnvironmentVariables.App));
                case @"\Process(??APP_WIN32_PROC??)\IO Data Bytes/sec":
                    return new RateCounterGauge(
                        reportAs,
                        "ioDataBytesRate",
                        AzureWebApEnvironmentVariables.App,
                        new SumUpGauge(
                            "ioDataBytesRate",
                            new PerformanceCounterFromJsonGauge(
                                "readIoBytes",
                                "readIoBytes",
                                AzureWebApEnvironmentVariables.App),
                            new PerformanceCounterFromJsonGauge(
                                "writeIoBytes",
                                "writeIoBytes",
                                AzureWebApEnvironmentVariables.App),
                            new PerformanceCounterFromJsonGauge(
                                "otherIoBytes",
                                "otherIoBytes",
                                AzureWebApEnvironmentVariables.App)));

                // $set = Get-Counter -ListSet Process
                // $set.Paths
                case @"\Process(??APP_WIN32_PROC??)\% Processor Time":
                    return new SumUpGauge(
                    reportAs,
                    new PerformanceCounterFromJsonGauge("kernelTime", "kernelTime", AzureWebApEnvironmentVariables.App),
                    new PerformanceCounterFromJsonGauge("userTime", "userTime", AzureWebApEnvironmentVariables.App));
                case @"\Process(??APP_WIN32_PROC??)\% User Time":
                    return new PerformanceCounterFromJsonGauge(
                        reportAs,
                        "userTime",
                        AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\Page Faults/ sec":
                    return new RateCounterGauge(
                        reportAs,
                        "pageFaults",
                        AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\Thread Count":
                    return new PerformanceCounterFromJsonGauge(
                    reportAs,
                    "threads",
                    AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\Handle Count":
                    return new PerformanceCounterFromJsonGauge(
                    reportAs,
                    "handles",
                    AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\IO Read Operations / sec":
                    return new RateCounterGauge(
                        reportAs,
                        "readIoOperations",
                        AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\IO Write Operations / sec":
                    return new RateCounterGauge(
                        reportAs,
                        "writeIoOperations",
                        AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\IO Other Operations / sec":
                    return new RateCounterGauge(
                        reportAs,
                        "otherIoOperations",
                        AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\IO Read Bytes / sec":
                    return new RateCounterGauge(
                        reportAs,
                        "readIoBytes",
                        AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\IO Write Bytes / sec":
                    return new RateCounterGauge(
                        reportAs,
                        "writeIoBytes",
                        AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\IO Other Bytes / sec":
                    return new RateCounterGauge(
                        reportAs,
                        "otherIoBytes",
                        AzureWebApEnvironmentVariables.App);
                default:
                    throw new ArgumentException("Performance counter was not found.", counterName);
            }
        }
    }
}