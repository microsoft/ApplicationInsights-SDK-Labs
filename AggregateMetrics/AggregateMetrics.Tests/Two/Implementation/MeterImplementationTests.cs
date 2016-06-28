namespace AggregateMetrics.Tests.Two
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MeterImplementationTests
    {
        [TestMethod]
        public void RateMeterMarkWorks()
        {
            var counter = new MeterImplementation("test", new TelemetryContext(), MeterAggregations.Rate);

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
        public void RateMeterMarkByValueWorks()
        {
            var counter = new MeterImplementation("test", new TelemetryContext(), MeterAggregations.Rate);

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
        public void RateMeterResetSetsValueToZero()
        {
            var counter = new MeterImplementation("test", new TelemetryContext(), MeterAggregations.Rate);

            for (int i = 0; i < 10; i++)
            {
                counter.Mark(2);
            }

            Assert.AreNotEqual(0, counter.GetValueAndReset().Value);
            Assert.AreEqual(0, counter.Value.Value);
        }

        [TestMethod]
        public void SumMeterMarkWorks()
        {
            var counter = new MeterImplementation("test", new TelemetryContext(), MeterAggregations.Sum);

            for (int i = 0; i < 10; i++)
            {
                counter.Mark();
            }

            var expected = 10;

            var value = counter.Value.Value;
            Assert.AreEqual(expected, value);
            value = counter.GetValueAndReset().Value;
            Assert.AreEqual(expected, value);
        }

        [TestMethod]
        public void SumMeterMarkByValueWorks()
        {
            var counter = new MeterImplementation("test", new TelemetryContext(), MeterAggregations.Sum);

            for (int i = 0; i < 10; i++)
            {
                counter.Mark(i);
            }

            var expected = 9 * (9 + 1) / 2;

            var value = counter.Value.Value;
            Assert.AreEqual(expected, value);
            value = counter.GetValueAndReset().Value;
            Assert.AreEqual(expected, value);
        }
    }
}
