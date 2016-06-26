namespace AggregateMetrics.Tests.Two
{
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Threading;

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

            var timer = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                counter.Mark(i);
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));

            timer.Stop();

            var expected = 9 * (9 + 1) / 2 / timer.Elapsed.TotalSeconds;

            var value = counter.Value.Value;
            Assert.IsTrue(expected - value < 0.001, "Actual: " + value + " Expected: " + expected);
            value = counter.GetValueAndReset().Value;
            Assert.IsTrue(expected - value < 0.001, "Actual: " + value + " Expected: " + expected);
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
