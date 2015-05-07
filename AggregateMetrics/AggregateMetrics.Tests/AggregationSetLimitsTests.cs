namespace CounterCollection.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AggregationSetLimitsTests : AggregationTests
    {
        [TestMethod]
        public void Property1ValuesAreLimitedToMaxCardinality()
        {
            var propertyValues = new List<string>();
            propertyValues.Add("Value1");
            propertyValues.Add("Value2");
            propertyValues.Add("Value3");
            propertyValues.Add("Value4");
            propertyValues.Add("Value4");
            propertyValues.Add("Value4");
            propertyValues.Add("Value5");
            propertyValues.Add("Value6");
            propertyValues.Add("Value6");
            propertyValues.Add("Value7");

            var client = new TelemetryClient();
            foreach (string prop in propertyValues)
            {
                client.TrackAggregateMetric("MyCounter", 123.00, prop);
            }

            Assert.AreEqual(1, AggregateMetrics.aggregationSets.Count);

            AggregationSet aggregationSet;
            AggregateMetrics.aggregationSets.TryGetValue(AggregateMetrics.aggregationSets.Keys.First(), out aggregationSet);

            var aggregations = aggregationSet.RemoveAggregations();

            Assert.AreEqual(5, aggregationSet.property1Values.Count);
            Assert.AreEqual(6, aggregations.Count);

            propertyValues.Add("other");

            foreach (MetricsBag counterData in aggregations.Values)
            {
                Assert.IsTrue(propertyValues.Contains(counterData.Property1));
            }
        }

        [TestMethod]
        public void Property2ValuesAreLimitedToMaxCardinality()
        {
            var propertyValues = new List<string>();
            propertyValues.Add("Value1");
            propertyValues.Add("Value2");
            propertyValues.Add("Value3");
            propertyValues.Add("Value4");
            propertyValues.Add("Value4");
            propertyValues.Add("Value4");
            propertyValues.Add("Value5");
            propertyValues.Add("Value6");
            propertyValues.Add("Value6");
            propertyValues.Add("Value7");

            var client = new TelemetryClient();
            foreach (string prop in propertyValues)
            {
                client.TrackAggregateMetric("MyCounter", 123.00, null, prop);
            }

            Assert.AreEqual(1, AggregateMetrics.aggregationSets.Count);

            AggregationSet aggregationSet;
            AggregateMetrics.aggregationSets.TryGetValue(AggregateMetrics.aggregationSets.Keys.First(), out aggregationSet);

            var aggregations = aggregationSet.RemoveAggregations();

            Assert.AreEqual(5, aggregationSet.property2Values.Count);
            Assert.AreEqual(6, aggregations.Count);

            propertyValues.Add("other");

            foreach (MetricsBag counterData in aggregations.Values)
            {
                Assert.IsTrue(propertyValues.Contains(counterData.Property2));
            }
        }

        [TestMethod]
        public void Property3ValuesAreLimitedToMaxCardinality()
        {
            var propertyValues = new List<string>();
            propertyValues.Add("Value1");
            propertyValues.Add("Value2");
            propertyValues.Add("Value3");
            propertyValues.Add("Value4");
            propertyValues.Add("Value4");
            propertyValues.Add("Value4");
            propertyValues.Add("Value5");
            propertyValues.Add("Value6");
            propertyValues.Add("Value6");
            propertyValues.Add("Value7");

            var client = new TelemetryClient();
            foreach (string prop in propertyValues)
            {
                client.TrackAggregateMetric("MyCounter", 123.00, null, null, prop);
            }

            Assert.AreEqual(1, AggregateMetrics.aggregationSets.Count);

            AggregationSet aggregationSet;
            AggregateMetrics.aggregationSets.TryGetValue(AggregateMetrics.aggregationSets.Keys.First(), out aggregationSet);

            var aggregations = aggregationSet.RemoveAggregations();

            Assert.AreEqual(5, aggregationSet.property3Values.Count);
            Assert.AreEqual(6, aggregations.Count);

            propertyValues.Add("other");

            foreach (MetricsBag counterData in aggregations.Values)
            {
                Assert.IsTrue(propertyValues.Contains(counterData.Property3));
            }
        }

        /// <summary>
        /// Tests the maximum number of aggregations in a set.
        /// 3 properties * (5 max values each + null value + 1 other value) = 21 max aggregations.
        /// </summary>
        [TestMethod]
        public void MaximumNumberOfAggregations()
        {
            var propertyValues = new string[] { "Value1", "Value2", "Value3", "Value4", "Value4", "Value5", null, "Value6", "Value6", "Value7", "Value8" };

            var client = new TelemetryClient();

            for (int i = 1; i <= 2; i++)
            {
                for (int p1 = 0; p1 < propertyValues.Count(); p1++)
                {
                    for (int p2 = 0; p2 < propertyValues.Count(); p2++)
                    {
                        for (int p3 = 0; p3 < propertyValues.Count(); p3++)
                        {
                            client.TrackAggregateMetric("My Counter", 50 * i, propertyValues[p1], propertyValues[p2], propertyValues[p3]);
                        }
                    }
                }
            }

            Assert.AreEqual(1, AggregateMetrics.aggregationSets.Count);

            AggregationSet aggregationSet;
            AggregateMetrics.aggregationSets.TryGetValue(AggregateMetrics.aggregationSets.Keys.First(), out aggregationSet);

            Assert.AreEqual(5, aggregationSet.property1Values.Count);
            Assert.AreEqual(5, aggregationSet.property2Values.Count);
            Assert.AreEqual(5, aggregationSet.property3Values.Count);

            var aggregations = aggregationSet.RemoveAggregations();

            Assert.AreEqual((Constants.MaxPropertyCardinality + 2) * (Constants.MaxPropertyCardinality + 2) * (Constants.MaxPropertyCardinality + 2), aggregations.Count);
        }
    }
}
