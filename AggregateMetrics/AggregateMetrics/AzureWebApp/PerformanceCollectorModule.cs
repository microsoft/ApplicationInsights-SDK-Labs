namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
    using Implementation;
    /// <summary>
    /// Telemetry module for collecting performance counters.
    /// </summary>
    public class PerformanceCollectorModule : ITelemetryModule
    {
        private readonly List<string> defaultCounters = new List<string>()
                                                            {
                                                                @"\Process(??APP_WIN32_PROC??)\% Processor Time",
                                                                @"\Memory\Available Bytes",
                                                                @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests/Sec",
                                                                @"\.NET CLR Exceptions(??APP_CLR_PROC??)\# of Exceps Thrown / sec",
                                                                @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Execution Time",
                                                                @"\Process(??APP_WIN32_PROC??)\Private Bytes",
                                                                @"\Process(??APP_WIN32_PROC??)\IO Data Bytes/sec",
                                                                @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests In Application Queue",
                                                                @"\Processor(_Total)\% Processor Time"
                                                            };

        public IList<PerformanceCounterCollectionRequest> Counters { get; private set; }

        /// <summary>
        /// Initializes the default performance counters.
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
            CounterFactory factory = new CounterFactory();

            foreach (string counter in defaultCounters)
                factory.GetCounter(counter);
        }

        /// <summary>
        /// Get specific performance counters that are not in the default counters list.
        /// </summary>
        /// <param name="performanceCounterRequests"> Requested performance counters.</param>
        public void AddCounter(params PerformanceCounterCollectionRequest[] performanceCounterRequests)
        {
            CounterFactory factory = new CounterFactory();

            foreach (PerformanceCounterCollectionRequest counter in performanceCounterRequests)
            {
                Counters.Add(counter);
                factory.GetCounter(counter.PerformanceCounter);
            }
        }
    }
}
