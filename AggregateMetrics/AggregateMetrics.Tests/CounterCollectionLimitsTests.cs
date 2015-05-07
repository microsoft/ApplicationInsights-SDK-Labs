namespace CounterCollection.Tests
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;

    [TestClass]
    public class CounterCollectionLimitsTests : AggregationTests
    {
        [TestMethod]
        public void CounterNameIsTruncatedToMaxLength()
        {
            var client = new TelemetryClient();
            client.TrackAggregateMetric("This name is longer than the maximum allowed length and will be truncated", 123.00);

            Assert.AreEqual(1, AggregateMetrics.aggregationSets.Count);

            AggregationSet aggregationSet;
            AggregateMetrics.aggregationSets.TryGetValue(AggregateMetrics.aggregationSets.Keys.First(), out aggregationSet);
            Assert.AreEqual("This name is lon", aggregationSet.Name);
        }

        [TestMethod]
        public void PropertyValueIsTruncatedToMaxLength()
        {
            var client = new TelemetryClient();
            client.TrackAggregateMetric("MyCounter", 123.00, "1. This is a long property value", "2. This is a long property value", "3. This is a long property value");

            Assert.AreEqual(1, AggregateMetrics.aggregationSets.Count);

            AggregationSet aggregationSet;
            AggregateMetrics.aggregationSets.TryGetValue(AggregateMetrics.aggregationSets.Keys.First(), out aggregationSet);

            MetricsBag counterData = aggregationSet.RemoveAggregations().First().Value;

            Assert.AreEqual("1. This is a lon", counterData.Property1);
            Assert.AreEqual("2. This is a lon", counterData.Property2);
            Assert.AreEqual("3. This is a lon", counterData.Property3);
        }
    }
}
