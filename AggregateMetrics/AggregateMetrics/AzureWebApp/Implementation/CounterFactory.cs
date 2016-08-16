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
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Execution Time":
                    return new PerformanceCounterFromJsonGauge(
                    reportAs,
                    "appRequestExecTime",
                    AzureWebApEnvironmentVariables.AspNet);
                case @"\Process(??APP_WIN32_PROC??)\Private Bytes":
                    return new PerformanceCounterFromJsonGauge(
                    reportAs,
                    "privateBytes",
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
                case @"\Process(??APP_WIN32_PROC??)\% Processor Time":
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
                case @"\Process(??APP_WIN32_PROC??)\Handle Count":
                    return new PerformanceCounterFromJsonGauge(
                    reportAs,
                    "handles",
                    AzureWebApEnvironmentVariables.App);
                case @"\Process(??APP_WIN32_PROC??)\Thread Count":
                    return new PerformanceCounterFromJsonGauge(
                    reportAs,
                    "threads",
                    AzureWebApEnvironmentVariables.App);

                //$set = Get-Counter -ListSet "ASP.NET Applications"
                //$set.Paths
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Anonymous Requests":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Anonymous Requests / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache Total Entries":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache Total Turnover Rate":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache Total Hits":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache Total Misses":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache Total Hit Ratio":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache API Entries":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache API Turnover Rate":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache API Hits":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache API Misses":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache API Hit Ratio":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Output Cache Entries":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Output Cache Turnover Rate":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Output Cache Hits":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Output Cache Misses":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Output Cache Hit Ratio":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Compilations Total":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Debugging Requests":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Errors During Preprocessing":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Errors During Compilation":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Errors During Execution":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Errors Unhandled During Execution":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Errors Unhandled During Execution / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Errors Total":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Errors Total / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Pipeline Instance Count":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Bytes In Total":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Bytes Out Total":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Executing":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Failed":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Not Found":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Not Authorized":
                //case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests In Application Queue":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Timed Out":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Succeeded":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Total":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Sessions Active":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Sessions Abandoned":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Sessions Timed Out":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Sessions Total":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Transactions Aborted":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Transactions Committed":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Transactions Pending":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Transactions Total":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Transactions / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Session State Server connections total":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Session SQL Server connections total":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Events Raised":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Events Raised / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Application Lifetime Events":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Application Lifetime Events / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Error Events Raised":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Error Events Raised / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Error Events Raised":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Error Events Raised / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Infrastructure Error Events Raised":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Infrastructure Error Events Raised / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Events Raised":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Events Raised / Sec":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Audit Success Events Raised":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Audit Failure Events Raised":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Membership Authentication Success":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Membership Authentication Failure":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Forms Authentication Success":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Forms Authentication Failure":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Viewstate MAC Validation Failure":
                //case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Execution Time":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Disconnected":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Rejected":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Wait Time":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache % Machine Memory Limit Used":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache % Process Memory Limit Used":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache Total Trims":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Cache API Trims":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Output Cache Trims":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\% Managed Processor Time(estimated)":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Managed Memory Used(estimated)":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Bytes In Total(WebSockets)":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Bytes Out Total(WebSockets)":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Executing(WebSockets)":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Failed(WebSockets)":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Succeeded(WebSockets)":
                case @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests Total(WebSockets)":


                // $set = Get-Counter -ListSet Process
                // $set.Paths
                //case @"\Process(??APP_WIN32_PROC??)\% Processor Time":
                case @"\Process(??APP_WIN32_PROC??)\% User Time":
                case @"\Process(??APP_WIN32_PROC??)\% Privileged Time":
                case @"\Process(??APP_WIN32_PROC??)\Virtual Bytes Peak":
                case @"\Process(??APP_WIN32_PROC??)\Virtual Bytes":
                case @"\Process(??APP_WIN32_PROC??)\Page Faults/ sec":
                case @"\Process(??APP_WIN32_PROC??)\Working Set Peak":
                case @"\Process(??APP_WIN32_PROC??)\Working Set":
                case @"\Process(??APP_WIN32_PROC??)\Page File Bytes Peak":
                case @"\Process(??APP_WIN32_PROC??)\Page File Bytes":
                //case @"\Process(??APP_WIN32_PROC??)\Private Bytes":
                //case @"\Process(??APP_WIN32_PROC??)\Thread Count":
                case @"\Process(??APP_WIN32_PROC??)\Priority Base":
                case @"\Process(??APP_WIN32_PROC??)\Elapsed Time":
                case @"\Process(??APP_WIN32_PROC??)\ID Process":
                case @"\Process(??APP_WIN32_PROC??)\Creating Process ID":
                case @"\Process(??APP_WIN32_PROC??)\Pool Paged Bytes":
                case @"\Process(??APP_WIN32_PROC??)\Pool Nonpaged Bytes":
                //case @"\Process(??APP_WIN32_PROC??)\Handle Count":
                case @"\Process(??APP_WIN32_PROC??)\IO Read Operations / sec":
                case @"\Process(??APP_WIN32_PROC??)\IO Write Operations / sec":
                case @"\Process(??APP_WIN32_PROC??)\IO Data Operations / sec":
                case @"\Process(??APP_WIN32_PROC??)\IO Other Operations / sec":
                case @"\Process(??APP_WIN32_PROC??)\IO Read Bytes / sec":
                case @"\Process(??APP_WIN32_PROC??)\IO Write Bytes / sec":
                case @"\Process(??APP_WIN32_PROC??)\IO Data Bytes / sec":
                case @"\Process(??APP_WIN32_PROC??)\IO Other Bytes / sec":
                case @"\Process(??APP_WIN32_PROC??)\Working Set - Private":


                //$set = Get - Counter - ListSet ".NET CLR Memory"
                //$set.Paths
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# Gen 0 Collections":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# Gen 1 Collections":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# Gen 2 Collections":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Promoted Memory from Gen 0":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Promoted Memory from Gen 1":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Gen 0 Promoted Bytes/ Sec":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Gen 1 Promoted Bytes/ Sec":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Promoted Finalization-Memory from Gen 0":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Process ID":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Gen 0 heap size":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Gen 1 heap size":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Gen 2 heap size":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Large Object Heap size":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Finalization Survivors":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# GC Handles":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\Allocated Bytes/ sec":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# Induced GC":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\% Time in GC":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# Bytes in all Heaps":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# Total committed Bytes":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# Total reserved Bytes":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# of Pinned Objects":
                case @"\.NET CLR Memory(??APP_CLR_PROC??)\# of Sink Blocks in use":

                default:
                    throw new ArgumentException("Performance counter was not found.", counterName);
            }
        }
    }
}
