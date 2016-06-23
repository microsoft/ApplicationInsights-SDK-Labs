namespace AggregateMetrics.Tests.Two
{
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MeterImplementationTests
    {
        [TestMethod]
        public void MeterMarkWorks()
        {
            var counter = new MeterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Mark();
            }

            Assert.AreEqual(1, counter.Value.Value);
            Assert.AreEqual(1, counter.GetValueAndReset().Value);
        }

        [TestMethod]
        public void MeterMarkByValueWorks()
        {
            var counter = new MeterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Mark(i);
            }

            var expected = 9 * (9 + 1) / 2 / 10.0;
            Assert.IsTrue(expected - counter.Value.Value < 0.001);
            Assert.IsTrue(expected - counter.GetValueAndReset().Value < 0.001);
        }

        [TestMethod]
        public void MeterResetSetsValueToZero()
        {
            var counter = new MeterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Mark(2);
            }

            Assert.AreEqual(2, counter.Value.Value);
            Assert.AreEqual(2, counter.GetValueAndReset().Value);
            Assert.AreEqual(0, counter.Value.Value);
        }
    }
}
