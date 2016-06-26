namespace AggregateMetrics.Tests.Two
{
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GaugeImplementationTests
    {
        [TestMethod]
        public void GaugeUsesValueFromDelegate()
        {
            var value = 10;

            var counter = new GaugeImplementation("test", new TelemetryContext(), () => { return value; });

            Assert.AreEqual(10, counter.Value.Value);
            Assert.AreEqual(10, counter.GetValueAndReset().Value);

            value = 20;

            Assert.AreEqual(20, counter.Value.Value);
            Assert.AreEqual(20, counter.GetValueAndReset().Value);
        }

        [TestMethod]
        public void CounteResetDoNotSetValueToZero()
        {
            var value = 10;

            var counter = new GaugeImplementation("test", new TelemetryContext(), () => { return value; });

            Assert.AreEqual(10, counter.Value.Value);
            Assert.AreEqual(10, counter.GetValueAndReset().Value);
            Assert.AreEqual(10, counter.Value.Value);
        }
    }
}
