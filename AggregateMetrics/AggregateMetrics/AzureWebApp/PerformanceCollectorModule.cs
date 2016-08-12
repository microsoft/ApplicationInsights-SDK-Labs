namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;

    /// <summary>
    /// Telemetry module for collecting performance counters.
    /// </summary>
    public class PerformanceCollectorModule : ITelemetryModule
    {
        private static readonly Regex DisallowedCharsInReportAsRegex = new Regex(
            @"[^a-zA-Z()/\\_. \t-]+",
            RegexOptions.Compiled);

        private static readonly Regex MultipleSpacesRegex = new Regex(
            @"[  ]+",
            RegexOptions.Compiled);

        /// <summary> 
        /// Initializes a new instance of the <see cref="PerformanceCollectorModule" /> class. 
        /// </summary> 
        public PerformanceCollectorModule()
        {
            this.Counters = new List<PerformanceCounterCollectionRequest>()
            {
                new PerformanceCounterCollectionRequest { PerformanceCounter = @"\Process(??APP_WIN32_PROC??)\% Processor Time", ReportAs = string.Empty },
                new PerformanceCounterCollectionRequest { PerformanceCounter = @"\Memory\Available Bytes", ReportAs = string.Empty },
                new PerformanceCounterCollectionRequest { PerformanceCounter = @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests/Sec", ReportAs = string.Empty },
                new PerformanceCounterCollectionRequest { PerformanceCounter = @"\.NET CLR Exceptions(??APP_CLR_PROC??)\# of Exceps Thrown / sec", ReportAs = string.Empty },
                new PerformanceCounterCollectionRequest { PerformanceCounter = @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Request Execution Time", ReportAs = string.Empty },
                new PerformanceCounterCollectionRequest { PerformanceCounter = @"\Process(??APP_WIN32_PROC??)\Private Bytes", ReportAs = string.Empty },
                new PerformanceCounterCollectionRequest { PerformanceCounter = @"\Process(??APP_WIN32_PROC??)\IO Data Bytes/sec", ReportAs = string.Empty },
                new PerformanceCounterCollectionRequest { PerformanceCounter = @"\ASP.NET Applications(??APP_W3SVC_PROC??)\Requests In Application Queue", ReportAs = string.Empty },
                new PerformanceCounterCollectionRequest { PerformanceCounter = @"\Processor(_Total)\% Processor Time", ReportAs = string.Empty }
            };
        }

        /// <summary>
        /// Gets custom performance counters set by user.
        /// </summary>
        public IList<PerformanceCounterCollectionRequest> Counters { get; private set; }

        /// <summary>
        /// Initializes the default performance counters.
        /// </summary>
        public void Initialize(TelemetryConfiguration configuration)
        {
            // TODO: Add tracing.
            if (!this.WebAppRunningInAzure())
            {
                return;
            }

            CounterFactory factory = new CounterFactory();

            foreach (var counter in this.Counters)
            {
                try
                {
                    string reportAs = this.SanitizeReportAs(counter.ReportAs, counter.PerformanceCounter);
                    reportAs = GetCounterReportAsName(counter.PerformanceCounter, reportAs);

                    ICounterValue c = factory.GetCounter(counter.PerformanceCounter, reportAs);
                    configuration.RegisterCounter(c);
                }
                catch
                {
                    // TODO: Add tracing.
                }
            }
        }

        /// <summary>
        /// Gets metric alias to be the value given by the user.
        /// </summary>
        /// <param name="counterName">Name of the counter to retrieve.</param>
        /// <param name="reportAs">Alias to report the counter.</param>
        /// <returns>Alias that will be used for the counter.</returns>
        private string GetCounterReportAsName(string counterName, string reportAs)
        {
            if (reportAs == null)
                return counterName;
            else
                return reportAs;
        }

        private bool WebAppRunningInAzure()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
        }

        private string SanitizeReportAs(string reportAs, string performanceCounter)
        {
            // Strip off disallowed characters.
            var newReportAs = DisallowedCharsInReportAsRegex.Replace(reportAs, string.Empty);
            newReportAs = MultipleSpacesRegex.Replace(newReportAs, " ");
            newReportAs = newReportAs.Trim();

            // If nothing is left, use default performance counter name.
            if (string.IsNullOrWhiteSpace(newReportAs))
            {
                return performanceCounter;
            }

            return newReportAs;
        }
    }
}