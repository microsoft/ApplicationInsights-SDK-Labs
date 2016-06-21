using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
{
    /// <summary>
    /// Interface represents the counter value.
    /// </summary>
    public interface ICounterValue
    {
        MetricTelemetry Value { get; }

        MetricTelemetry GetValueAndReset();
    }
}
