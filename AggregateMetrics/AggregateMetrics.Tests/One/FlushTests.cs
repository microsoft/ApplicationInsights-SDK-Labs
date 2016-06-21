namespace AggregateMetrics.Tests.One
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One;
    [TestClass]
    public class FlushTests
    {
        [TestMethod]
        public void CardinalityLimitsPersistAcrossFlushes()
        {
            var client = new TelemetryClient();

            client.TrackAggregateMetric("Test1", 123, "Prop1");
            client.TrackAggregateMetric("Test1", 123, "Prop2");
            client.TrackAggregateMetric("Test1", 123, "Prop3");
            client.TrackAggregateMetric("Test1", 123, "Prop4");
            client.TrackAggregateMetric("Test1", 123, "Prop5");
            client.TrackAggregateMetric("Test1", 123, "Prop6");
            client.TrackAggregateMetric("Test1", 123, "Prop7");

            Assert.AreEqual(6, AggregateMetrics.aggregationSets.Values.First().aggregations.Count);

            AggregateMetrics.FlushImpl();

            client.TrackAggregateMetric("Test1", 123, "Prop8");
            client.TrackAggregateMetric("Test1", 123, "Prop9");
            client.TrackAggregateMetric("Test1", 123, "Prop10");

            Assert.AreEqual(1, AggregateMetrics.aggregationSets.Values.First().aggregations.Count);

            Assert.AreEqual("other", AggregateMetrics.aggregationSets.Values.First().aggregations.First().Value.Property1);
        }
    }
}
