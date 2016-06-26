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
            Thread.Sleep(TimeSpan.FromSeconds(1));

            var expected = 10;

            var value = counter.Value.Value;
            Assert.IsTrue(Math.Abs(expected - value) < 1, "Actual: " + value + " Expected: " + expected);
            value = counter.GetValueAndReset().Value;
            Assert.IsTrue(Math.Abs(expected - value) < 1, "Actual: " + value + " Expected: " + expected);
        }

        [TestMethod]
        public void MeterMarkByValueWorks()
        {
            var counter = new MeterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Mark(i);
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));

            var expected = 9 * (9 + 1) / 2; // it's rate per second and only one second has passed

            var value = counter.Value.Value;
            Assert.IsTrue(Math.Abs(expected - value) < 1, "Actual: " + value + " Expected: " + expected);
            value = counter.GetValueAndReset().Value;
            Assert.IsTrue(Math.Abs(expected - value) < 1, "Actual: " + value + " Expected: " + expected);
        }

        [TestMethod]
        public void MeterResetSetsValueToZero()
        {
            var counter = new MeterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Mark(2);
            }


            Assert.AreNotEqual(0, counter.GetValueAndReset().Value);
            Assert.AreEqual(0, counter.Value.Value);
        }
    }
}
