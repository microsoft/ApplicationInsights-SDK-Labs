using Microsoft.ApplicationInsights.DataContracts;
using System;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    public struct TimeDependentCounter
    {
        public MetricTelemetry MetricTelemetry;

        public DateTime DateTime;
    }
}
