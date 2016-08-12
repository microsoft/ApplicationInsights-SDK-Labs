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

        public PerformanceCollectorModule()
        {
            this.Counters = new List<PerformanceCounterCollectionRequest>();
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
            if (!this.WebAppRunningInAzure())
            {
                return;
            }

            CounterFactory factory = new CounterFactory();

            foreach (string counter in this.defaultCounters)
            {
                try
                {
                    ICounterValue c = factory.GetCounter(counter);
                    configuration.RegisterCounter(c);
                }
                catch
                {
                    // TODO: Add tracing.
                }
            }

            foreach (var counter in this.Counters)
            {
                try
                {
                    ICounterValue c = counter.ReportAs == null ? factory.GetCounter(counter.PerformanceCounter) : factory.GetCounter(counter.PerformanceCounter, this.SanitizeReportAs(counter.ReportAs, counter.PerformanceCounter));
                    configuration.RegisterCounter(c);
                }
                catch
                {
                    // TODO: Add tracing.
                }
            }
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
