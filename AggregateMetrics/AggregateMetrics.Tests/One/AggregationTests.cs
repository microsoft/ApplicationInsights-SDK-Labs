namespace AggregateMetrics.Tests.One
{
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public abstract class AggregationTests : UnitTests
    {
        [TestInitialize]
        public void Initialize()
        {
            AggregateMetricsTelemetryModule.IsTimerFlushEnabled = false;
            AggregateMetrics.Clear();
        }

        [TestCleanup]
        public void Cleanup()
        {
            AggregateMetrics.Clear();
        }
    }
}
