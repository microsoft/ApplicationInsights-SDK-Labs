namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.Threading;
    using System.Xml;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Wcf.Implementation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MessageCorrelatorTests
    {
        [TestMethod]
        public void WhenAddIsCalledWithNullId_ArgumentNullExceptionIsThrown()
        {
            MessageCorrelator correlator = new MessageCorrelator();
            var telemetry = new DependencyTelemetry();
            bool failed = false;
            try
            {
                correlator.Add(null, telemetry, TimeSpan.FromMilliseconds(100));
            }
            catch (ArgumentNullException)
            {
                failed = true;
            }

            Assert.IsTrue(failed, "ArgumentNullException was not thrown");
        }

        [TestMethod]
        public void WhenAddIsCalledWithNullTelemetry_ArgumentNullExceptionIsThrown()
        {
            MessageCorrelator correlator = new MessageCorrelator();
            bool failed = false;
            try
            {
                correlator.Add(new UniqueId(), null, TimeSpan.FromMilliseconds(100));
            }
            catch (ArgumentNullException)
            {
                failed = true;
            }

            Assert.IsTrue(failed, "ArgumentNullException was not thrown");
        }

        [TestMethod]
        public void WhenMessageIsAdded_TryLookupReturnsTrue()
        {
            MessageCorrelator correlator = new MessageCorrelator();
            var telemetry = new DependencyTelemetry();

            var id = new UniqueId();
            correlator.Add(id, telemetry, TimeSpan.FromMilliseconds(100));

            DependencyTelemetry result;
            Assert.IsTrue(correlator.TryLookup(id, out result));
            Assert.AreSame(telemetry, result);
        }

        [TestMethod]
        public void WhenTryLookupReturnsTrue_MessageIsRemoved()
        {
            MessageCorrelator correlator = new MessageCorrelator();
            var telemetry = new DependencyTelemetry();

            var id = new UniqueId();
            correlator.Add(id, telemetry, TimeSpan.FromMilliseconds(100));

            DependencyTelemetry result;
            Assert.IsTrue(correlator.TryLookup(id, out result));

            Assert.IsFalse(correlator.TryLookup(id, out result));
        }

        [TestMethod]
        public void WhenMessageIsAdded_AndNotRemoved_TimeoutCallbackIsFired()
        {
            UniqueId timeoutId = null;
            DependencyTelemetry timeoutTelemetry = null;
            ManualResetEvent timeoutEvent = new ManualResetEvent(false);

            MessageCorrelator correlator = new MessageCorrelator(
                (messageId, dependencyObj) =>
                {
                    timeoutId = messageId;
                    timeoutTelemetry = dependencyObj;
                    timeoutEvent.Set();
                });
            var telemetry = new DependencyTelemetry();

            var id = new UniqueId();
            correlator.Add(id, telemetry, TimeSpan.FromMilliseconds(100));
            Assert.IsTrue(timeoutEvent.WaitOne(200));
            Assert.AreEqual(id, timeoutId);
            Assert.AreEqual(telemetry, timeoutTelemetry);
        }

        [TestMethod]
        public void WhenMessageIsAdded_AndRemoved_TimeoutCallbackIsNotFired()
        {
            ManualResetEvent timeoutEvent = new ManualResetEvent(false);

            MessageCorrelator correlator = new MessageCorrelator(
                (messageId, dependencyObj) =>
                {
                    timeoutEvent.Set();
                });
            var telemetry = new DependencyTelemetry();

            // add and remove right away
            var id = new UniqueId();
            correlator.Add(id, telemetry, TimeSpan.FromMilliseconds(100));
            DependencyTelemetry result;
            Assert.IsTrue(correlator.TryLookup(id, out result));

            // should timeout
            Assert.IsFalse(timeoutEvent.WaitOne(200));
        }

        [TestMethod]
        public void WhenDisposed_AddThrowsException()
        {
            MessageCorrelator correlator = new MessageCorrelator();
            correlator.Dispose();

            var id = new UniqueId();
            bool failed = false;
            try
            {
                correlator.Add(id, new DependencyTelemetry(), TimeSpan.FromMilliseconds(100));
            }
            catch (ObjectDisposedException)
            {
                failed = true;
            }

            Assert.IsTrue(failed, "Add did not throw ObjectDisposedException");
        }
    }
}
