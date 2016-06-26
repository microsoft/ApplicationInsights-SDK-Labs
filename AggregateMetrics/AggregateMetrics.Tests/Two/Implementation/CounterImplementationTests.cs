namespace AggregateMetrics.Tests.Two
{
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CounterImplementationTests
    {
        [TestMethod]
        public void CounterIncrementWorks()
        {
            var counter = new CounterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Increment();
            }

            Assert.AreEqual(10, counter.Value.Value);
            Assert.AreEqual(10, counter.GetValueAndReset().Value);
        }

        [TestMethod]
        public void CounterIncrementByValueWorks()
        {
            var counter = new CounterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Increment(2);
            }

            Assert.AreEqual(20, counter.Value.Value);
            Assert.AreEqual(20, counter.GetValueAndReset().Value);
        }

        [TestMethod]
        public void CounterDecrementWorks()
        {
            var counter = new CounterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Decrement();
            }

            Assert.AreEqual(-10, counter.Value.Value);
            Assert.AreEqual(-10, counter.GetValueAndReset().Value);
        }

        [TestMethod]
        public void CounteDecrementByValueWorks()
        {
            var counter = new CounterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Decrement(2);
            }

            Assert.AreEqual(-20, counter.Value.Value);
            Assert.AreEqual(-20, counter.GetValueAndReset().Value);
        }

        [TestMethod]
        public void CounteResetDoNotSetValueToZero()
        {
            var counter = new CounterImplementation("test", new TelemetryContext());

            for (int i = 0; i < 10; i++)
            {
                counter.Decrement(2);
            }

            Assert.AreEqual(-20, counter.Value.Value);
            Assert.AreEqual(-20, counter.GetValueAndReset().Value);
            Assert.AreEqual(-20, counter.Value.Value);
        }
    }
}
