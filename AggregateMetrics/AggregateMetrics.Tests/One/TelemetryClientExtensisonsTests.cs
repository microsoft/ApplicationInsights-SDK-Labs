namespace CounterCollection.One.Tests
{
    using Microsoft.ApplicationInsights;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One;
    using Microsoft.ApplicationInsights.Extensibility;

    [TestClass]
    public class TelemetryClientExtensisonsTests
    {
        [TestMethod]
        public void SimpleUsageExample()
        {
            TelemetryConfiguration configuraiton = new TelemetryConfiguration();

            TelemetryClient client = new TelemetryClient(configuraiton);

            var simpleCounter = client.Counter("test");
            var counters = configuraiton.GetCounters();

            Assert.AreEqual(1, counters.Count);

            for (int i = 0; i < 10; i++)
            {
                simpleCounter.Increment();
            }

            Assert.AreEqual(10, counters["test"].Value.Value);
            Assert.AreEqual(1, counters["test"].Value.Count);
        }
    }
}