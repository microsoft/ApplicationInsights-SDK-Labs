using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
using Microsoft.ApplicationInsights.Wcf.Tests.Integration;
using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class ClientChannelOpenTrackerTests
    {
        const String SvcUrl = "http://localhost/MyService.svc";

        [TestMethod]
        public void WhenOpenSucceedsTelemetryEventIsWritten()
        {
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new MockClientChannel(SvcUrl);
            var tracker = new ClientChannelOpenTracker(client, typeof(ISimpleService));

            tracker.Initialize(channel);
            channel.SimulateOpen(TimeSpan.FromMilliseconds(100), false);

            var telemetry = TestTelemetryChannel.CollectedData()
                                                .OfType<DependencyTelemetry>()
                                                .FirstOrDefault();

            Assert.IsNotNull(telemetry, "Did not write telemetry event");
            Assert.AreEqual(SvcUrl, telemetry.Name);
            Assert.AreEqual("localhost", telemetry.Target);
            Assert.AreEqual(DependencyConstants.WcfChannelOpen, telemetry.Type);
            Assert.AreEqual(typeof(ISimpleService).FullName, telemetry.Data);
            Assert.AreEqual(true, telemetry.Success.Value);
        }

        [TestMethod]
        public void WhenOpenFailsTelemetryEventIsWritten()
        {
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new MockClientChannel(SvcUrl);
            var tracker = new ClientChannelOpenTracker(client, typeof(ISimpleService));

            tracker.Initialize(channel);
            channel.SimulateOpen(TimeSpan.FromMilliseconds(100), true);

            var telemetry = TestTelemetryChannel.CollectedData()
                                                .OfType<DependencyTelemetry>()
                                                .FirstOrDefault();

            Assert.IsNotNull(telemetry, "Did not write telemetry event");
            Assert.AreEqual(SvcUrl, telemetry.Name);
            Assert.AreEqual("localhost", telemetry.Target);
            Assert.AreEqual(DependencyConstants.WcfChannelOpen, telemetry.Type);
            Assert.AreEqual(typeof(ISimpleService).FullName, telemetry.Data);
            Assert.AreEqual(false, telemetry.Success.Value);
        }

        [TestMethod]
        public void WhenOpenSucceedsEventsAreUnhooked()
        {
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new MockClientChannel(SvcUrl);
            var tracker = new ClientChannelOpenTracker(client, typeof(ISimpleService));

            tracker.Initialize(channel);
            channel.SimulateOpen(TimeSpan.FromMilliseconds(100), false);

            Assert.IsFalse(channel.OpeningIsHooked(), "Opening event still hooked");
            Assert.IsFalse(channel.OpenedIsHooked(), "Opened event still hooked");
            Assert.IsFalse(channel.FaultedIsHooked(), "Faulted event still hooked");
        }

        [TestMethod]
        public void WhenOpenFailsEventsAreUnhooked()
        {
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new MockClientChannel(SvcUrl);
            var tracker = new ClientChannelOpenTracker(client, typeof(ISimpleService));

            tracker.Initialize(channel);
            channel.SimulateOpen(TimeSpan.FromMilliseconds(100), true);

            Assert.IsFalse(channel.OpeningIsHooked(), "Opening event still hooked");
            Assert.IsFalse(channel.OpenedIsHooked(), "Opened event still hooked");
            Assert.IsFalse(channel.FaultedIsHooked(), "Faulted event still hooked");
        }
    }
}
