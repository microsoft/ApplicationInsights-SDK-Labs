using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
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

        public void Initialize(TelemetryConfiguration configuration)
        {
            DefaultCounters defaultCounters = new DefaultCounters();
            defaultCounters.Initialize(configuration);
        }
    }
}
