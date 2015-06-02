namespace AggregateMetrics.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MetricTelemetryExtensionsTests
    {
        [TestMethod]
        public void AddPropertiesNoPropertiesTest()
        {
            MetricsBag bag = new MetricsBag();
            MetricTelemetry metric = new MetricTelemetry();

            metric.AddProperties(bag, "p1", "p2", "p3");

            Assert.AreEqual(0, metric.Properties.Count);
        }

        [TestMethod]
        public void AddPropertiesOnePropertyTest()
        {
            MetricsBag bag = new MetricsBag("prop1", null, null);
            MetricTelemetry metric = new MetricTelemetry();

            metric.AddProperties(bag, "p1", "p2", "p3");

            Assert.AreEqual(1, metric.Properties.Count);
            for (int i = 1; i <= metric.Properties.Count; i++)
            {
                KeyValuePair<string, string> prop = metric.Properties.Skip(i - 1).First();
                Assert.AreEqual("p" + i, prop.Key);
                Assert.AreEqual("prop" + i, prop.Value);
            }
        }

        [TestMethod]
        public void AddPropertiesTwoProperteriesTest()
        {
            MetricsBag bag = new MetricsBag("prop1", "prop2", null);
            MetricTelemetry metric = new MetricTelemetry();

            metric.AddProperties(bag, "p1", "p2", "p3");

            Assert.AreEqual(2, metric.Properties.Count);
            for (int i = 1; i <= metric.Properties.Count; i++)
            {
                KeyValuePair<string, string> prop = metric.Properties.Skip(i - 1).First();
                Assert.AreEqual("p" + i, prop.Key);
                Assert.AreEqual("prop" + i, prop.Value);
            }
        }

        [TestMethod]
        public void AddPropertiesThreeProperteriesTest()
        {
            MetricsBag bag = new MetricsBag("prop1", "prop2", "prop3");
            MetricTelemetry metric = new MetricTelemetry();

            metric.AddProperties(bag, "p1", "p2", "p3");

            Assert.AreEqual(3, metric.Properties.Count);
            for (int i = 1; i <= metric.Properties.Count; i++)
            {
                KeyValuePair<string, string> prop = metric.Properties.Skip(i - 1).First();
                Assert.AreEqual("p" + i, prop.Key);
                Assert.AreEqual("prop" + i, prop.Value);
            }
        }
    }
}
