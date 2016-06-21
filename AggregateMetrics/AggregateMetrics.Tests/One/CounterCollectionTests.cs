namespace AggregateMetrics.Tests.One
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One;
    [TestClass]
    public class CounterCollectionTests : UnitTests
    {
        [TestMethod]
        public void SimpleTrackCounter()
        {
            var client = new TelemetryClient();

            client.TrackAggregateMetric("Test", 123.00);
        }

        [TestMethod]
        public void SimpleTrackCounterSingleProperty()
        {
            var client = new TelemetryClient();

            client.TrackAggregateMetric("Test", 123.00, "Machine1");
        }

        [TestMethod]
        public void SimpleTrackCounterTwoProperties()
        {
            var client = new TelemetryClient();

            client.TrackAggregateMetric("Test", 123.00, "Machine1", "Role1");
        }

        [TestMethod]
        public void SimpleTrackCounterThreeProperties()
        {
            var client = new TelemetryClient();

            client.TrackAggregateMetric("Test", 123.00, "Machine1", "Role1", "ScaryCustomer");
        }

        [TestMethod]
        public void NullName()
        {
            var client = new TelemetryClient();

            client.TrackAggregateMetric(null, 123.00);
        }

        [TestMethod]
        public void EmptyName()
        {
            var client = new TelemetryClient();

            client.TrackAggregateMetric("", 123.00);
        }

        [TestMethod]
        public void Aggregation()
        {
            var client = new TelemetryClient();

            client.TrackAggregateMetric("Test", 123.00);

            // Wait for aggregation timer
            Thread.Sleep(AggregateMetricsTelemetryModule.FlushIntervalSeconds * 1000 * 2);
        }
    }
}
