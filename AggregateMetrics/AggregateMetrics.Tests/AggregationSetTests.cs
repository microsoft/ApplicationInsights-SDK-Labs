namespace CounterCollection.Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AggregationSetTests : AggregationTests
    {
        [TestMethod]
        public void AggregationSetKeyTelemetryClient()
        {
            var telemetryClient1 = new TelemetryClient();
            var telemetryClient2 = new TelemetryClient();
            int key1 = AggregationSet.GetKey(telemetryClient1, "Agg1");
            int key2 = AggregationSet.GetKey(telemetryClient1, "Agg1");
            int key3 = AggregationSet.GetKey(telemetryClient2, "Agg1");

            Assert.AreEqual(key1, key2);
            Assert.AreNotEqual(key2, key3);
        }

        [TestMethod]
        public void AggregationSetKeyCounterName()
        {
            var telemetryClient = new TelemetryClient();
            int key1 = AggregationSet.GetKey(telemetryClient, "Agg1");
            int key2 = AggregationSet.GetKey(telemetryClient, "Agg1");
            int key3 = AggregationSet.GetKey(telemetryClient, "Agg2");

            Assert.AreEqual(key1, key2);
            Assert.AreNotEqual(key2, key3);
        }

        [TestMethod]
        public void AggregationSetKeyCounterNameSingleProp()
        {
            var telemetryClient = new TelemetryClient();
            int key1 = AggregationSet.GetKey(telemetryClient, "Agg1");
            int key2 = AggregationSet.GetKey(telemetryClient, "Agg1");
            int key3 = AggregationSet.GetKey(telemetryClient, "Agg1");

            Assert.AreEqual(key1, key2);
            Assert.AreEqual(key2, key3);
        }

        [TestMethod]
        public void BasicCreate()
        {
            var telemetryClient = new TelemetryClient();
            var aggregationSet = new AggregationSet(telemetryClient, "CounterName");

            Assert.AreEqual("CounterName", aggregationSet.Name);
        }

        [TestMethod]
        public void BasicCreateCounterData()
        {
            const double value = 15.5;
            string prop1Value = "Prop1-Value";

            var telemetryClient = new TelemetryClient();
            var aggregationSet = new AggregationSet(telemetryClient, "CounterName");
            aggregationSet.AddAggregation(value, prop1Value);

            Assert.AreEqual("CounterName", aggregationSet.Name);

            var aggregations = aggregationSet.RemoveAggregations();

            MetricsBag counterData = aggregations.Values.First();
            Assert.AreEqual("Prop1-Value", counterData.Property1);

            Assert.AreEqual(value, counterData.CalculateAggregation().Average);
        }

        [TestMethod]
        public void BucketizationOfAggregationSets()
        {
            var propertyValues = new string[] { "Value1", "Value2", "Value3", "Value4", "Value5", null, "Value6" };

            const int seedValue = 50;
            const int iterations = 50;

            var client = new TelemetryClient();

            var perfCollector = new PerfCollector(this.TestContext);

            for (int i = 1; i <= iterations; i++)
            {
                for (int p1 = 0; p1 < propertyValues.Count(); p1++)
                {
                    for (int p2 = 0; p2 < propertyValues.Count(); p2++)
                    {
                        for (int p3 = 0; p3 < propertyValues.Count(); p3++)
                        {
                            client.TrackAggregateMetric("My Counter", seedValue * i, propertyValues[p1], propertyValues[p2], propertyValues[p3]);
                        }
                    }
                }
            }

            Assert.AreEqual(1, AggregateMetrics.aggregationSets.Count);

            perfCollector.StopAndSubmitPerfData();

            AggregationSet aggregationSet;
            AggregateMetrics.aggregationSets.TryGetValue(AggregateMetrics.aggregationSets.Keys.First(), out aggregationSet);

            Assert.AreEqual(5, aggregationSet.property1Values.Count);
            Assert.AreEqual(5, aggregationSet.property2Values.Count);
            Assert.AreEqual(5, aggregationSet.property3Values.Count);

            var aggregations = aggregationSet.RemoveAggregations();

            Assert.AreEqual((Constants.MaxPropertyCardinality + 2) * (Constants.MaxPropertyCardinality + 2) * (Constants.MaxPropertyCardinality + 2), aggregations.Count);

            foreach (MetricsBag counter in aggregations.Values)
            {
                Assert.AreEqual(iterations, counter.CalculateAggregation().Count);
                Assert.AreEqual(seedValue * (iterations + 1) / 2, counter.CalculateAggregation().Average);
            }
        }
    }
}
