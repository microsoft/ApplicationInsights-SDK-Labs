using Microsoft.ApplicationInsights.DataContracts;

namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
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
