using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
using Microsoft.ApplicationInsights.Wcf.Tests.Integration;
using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    public abstract class ChannelTestBase<TChannel> where TChannel : IChannel
    {
        public const String SvcUrl = "http://localhost/MyService.svc";
        public const String HostName = "localhost";

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelManagerIsNull_ConstructorThrowsException()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            bool failed = false;
            try
            {
                var channel = GetChannel(null, innerChannel);
            } catch ( ArgumentNullException )
            {
                failed = true;
            }
            Assert.IsTrue(failed, "Constructor did not throw ArgumentNullException");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenInnerChannelIsNull_ConstructorThrowsException()
        {

            var manager = new ClientChannelManager(new TelemetryClient(), typeof(ISimpleService));
            bool failed = false;
            try
            {
                var channel = GetChannel(manager, null);
            } catch ( ArgumentNullException )
            {
                failed = true;
            }
            Assert.IsTrue(failed, "Constructor did not throw ArgumentNullException");
        }


        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsCreated_InnerChannelRemoteAddressIsReturned()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            // IChannel does not define RemoteAddress, so cheat

            var prop = channel.GetType().GetProperty("RemoteAddress", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            Assert.AreEqual(innerChannel.RemoteAddress, prop.GetValue(channel, null));
        }

        //
        // Event Handling
        //

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpened_InnerChannelEventsAreHooked()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open();

            Assert.IsTrue(innerChannel.OpeningIsHooked(), "Opening event is not hooked");
            Assert.IsTrue(innerChannel.OpenedIsHooked(), "Opened event is not hooked");
            Assert.IsTrue(innerChannel.ClosingIsHooked(), "Closing event is not hooked");
            Assert.IsTrue(innerChannel.ClosedIsHooked(), "Closed event is not hooked");
            Assert.IsTrue(innerChannel.FaultedIsHooked(), "Faulted event is not hooked");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedWithTimeout_InnerChannelEventsAreHooked()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open(TimeSpan.FromSeconds(10));

            Assert.IsTrue(innerChannel.OpeningIsHooked(), "Opening event is not hooked");
            Assert.IsTrue(innerChannel.OpenedIsHooked(), "Opened event is not hooked");
            Assert.IsTrue(innerChannel.ClosingIsHooked(), "Closing event is not hooked");
            Assert.IsTrue(innerChannel.ClosedIsHooked(), "Closed event is not hooked");
            Assert.IsTrue(innerChannel.FaultedIsHooked(), "Faulted event is not hooked");
        }


        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedAsync_InnerChannelEventsAreHooked()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            var result = channel.BeginOpen(null, null);
            channel.EndOpen(result);

            Assert.IsTrue(innerChannel.OpeningIsHooked(), "Opening event is not hooked");
            Assert.IsTrue(innerChannel.OpenedIsHooked(), "Opened event is not hooked");
            Assert.IsTrue(innerChannel.ClosingIsHooked(), "Closing event is not hooked");
            Assert.IsTrue(innerChannel.ClosedIsHooked(), "Closed event is not hooked");
            Assert.IsTrue(innerChannel.FaultedIsHooked(), "Faulted event is not hooked");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedAsyncWithTimeout_InnerChannelEventsAreHooked()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            var result = channel.BeginOpen(TimeSpan.FromSeconds(10), null, null);
            channel.EndOpen(result);

            Assert.IsTrue(innerChannel.OpeningIsHooked(), "Opening event is not hooked");
            Assert.IsTrue(innerChannel.OpenedIsHooked(), "Opened event is not hooked");
            Assert.IsTrue(innerChannel.ClosingIsHooked(), "Closing event is not hooked");
            Assert.IsTrue(innerChannel.ClosedIsHooked(), "Closed event is not hooked");
            Assert.IsTrue(innerChannel.FaultedIsHooked(), "Faulted event is not hooked");
        }

        [TestMethod]
        public void WhenChannelIsClosed_InnerChannelEventsAreUnhooked()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open();
            channel.Close();

            Assert.IsFalse(innerChannel.OpeningIsHooked(), "Opening event is hooked");
            Assert.IsFalse(innerChannel.OpenedIsHooked(), "Opened event is hooked");
            Assert.IsFalse(innerChannel.ClosingIsHooked(), "Closing event is hooked");
            Assert.IsFalse(innerChannel.ClosedIsHooked(), "Closed event is hooked");
            Assert.IsFalse(innerChannel.FaultedIsHooked(), "Faulted event is hooked");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsClosedWithTimeout_InnerChannelEventsAreUnhooked()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open();
            channel.Close(TimeSpan.FromSeconds(1));

            Assert.IsFalse(innerChannel.OpeningIsHooked(), "Opening event is hooked");
            Assert.IsFalse(innerChannel.OpenedIsHooked(), "Opened event is hooked");
            Assert.IsFalse(innerChannel.ClosingIsHooked(), "Closing event is hooked");
            Assert.IsFalse(innerChannel.ClosedIsHooked(), "Closed event is hooked");
            Assert.IsFalse(innerChannel.FaultedIsHooked(), "Faulted event is hooked");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsClosedAsync_InnerChannelEventsAreUnhooked()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open();
            var result = channel.BeginClose(null, null);
            channel.EndClose(result);

            Assert.IsFalse(innerChannel.OpeningIsHooked(), "Opening event is hooked");
            Assert.IsFalse(innerChannel.OpenedIsHooked(), "Opened event is hooked");
            Assert.IsFalse(innerChannel.ClosingIsHooked(), "Closing event is hooked");
            Assert.IsFalse(innerChannel.ClosedIsHooked(), "Closed event is hooked");
            Assert.IsFalse(innerChannel.FaultedIsHooked(), "Faulted event is hooked");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsClosedAsyncWithTimeout_InnerChannelEventsAreUnhooked()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open();
            var result = channel.BeginClose(TimeSpan.FromSeconds(10), null, null);
            channel.EndClose(result);

            Assert.IsFalse(innerChannel.OpeningIsHooked(), "Opening event is hooked");
            Assert.IsFalse(innerChannel.OpenedIsHooked(), "Opened event is hooked");
            Assert.IsFalse(innerChannel.ClosingIsHooked(), "Closing event is hooked");
            Assert.IsFalse(innerChannel.ClosedIsHooked(), "Closed event is hooked");
            Assert.IsFalse(innerChannel.FaultedIsHooked(), "Faulted event is hooked");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsAborted_InnerChannelEventsAreUnhooked()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open();
            channel.Abort();

            Assert.IsFalse(innerChannel.OpeningIsHooked(), "Opening event is hooked");
            Assert.IsFalse(innerChannel.OpenedIsHooked(), "Opened event is hooked");
            Assert.IsFalse(innerChannel.ClosingIsHooked(), "Closing event is hooked");
            Assert.IsFalse(innerChannel.ClosedIsHooked(), "Closed event is hooked");
            Assert.IsFalse(innerChannel.FaultedIsHooked(), "Faulted event is hooked");
        }

        //
        // Channel.Open Telemetry
        //

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpened_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open();

            CheckOpenDependencyWritten(typeof(ISimpleService), true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedWithTimeout_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open(TimeSpan.FromSeconds(10));

            CheckOpenDependencyWritten(typeof(ISimpleService), true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedAsync_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            var result = channel.BeginOpen(null, null);
            channel.EndOpen(result);

            CheckOpenDependencyWritten(typeof(ISimpleService), true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedAsyncWithTimeout_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            var result = channel.BeginOpen(TimeSpan.FromSeconds(10), null, null);
            channel.EndOpen(result);

            CheckOpenDependencyWritten(typeof(ISimpleService), true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpened_TelemetryIsWritten_Failure()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            innerChannel.FailOpen = true;

            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            bool failed = false;
            try
            {
                channel.Open();
            } catch ( Exception )
            {
                failed = true;
            }
            Assert.IsTrue(failed, "Open() did not throw exception");

            CheckOpenDependencyWritten(typeof(ISimpleService), false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedWithTimeout_TelemetryIsWritten_Failure()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            innerChannel.FailOpen = true;

            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            bool failed = false;
            try
            {
                channel.Open(TimeSpan.FromSeconds(10));
            } catch ( Exception )
            {
                failed = true;
            }
            Assert.IsTrue(failed, "Open() did not throw exception");

            CheckOpenDependencyWritten(typeof(ISimpleService), false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedAsync_TelemetryIsWritten_FailureInBegin()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            innerChannel.FailBeginOpen = true;

            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            bool failed = false;
            try
            {
                channel.BeginOpen(null, null);
            } catch ( Exception )
            {
                failed = true;
            }
            Assert.IsTrue(failed, "BeginOpen() did not throw exception");

            CheckOpenDependencyWritten(typeof(ISimpleService), false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedAsyncWithTimeout_TelemetryIsWritten_FailureInBegin()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            innerChannel.FailBeginOpen = true;

            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            bool failed = false;
            try
            {
                channel.BeginOpen(TimeSpan.FromSeconds(10), null, null);
            } catch ( Exception )
            {
                failed = true;
            }
            Assert.IsTrue(failed, "BeginOpen() did not throw exception");

            CheckOpenDependencyWritten(typeof(ISimpleService), false);
        }



        internal abstract TChannel GetChannel(IChannel channel, Type contract);
        internal abstract TChannel GetChannel(IChannelManager manager, IChannel channel);

        private void CheckOpenDependencyWritten(Type contract, bool success)
        {
            var dependency = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().FirstOrDefault();
            Assert.IsNotNull(dependency, "Did not write dependency event");
            Assert.AreEqual(SvcUrl, dependency.Name);
            Assert.AreEqual(HostName, dependency.Target);
            Assert.AreEqual(DependencyConstants.WcfChannelOpen, dependency.Type);
            Assert.AreEqual(contract.FullName, dependency.Data);
            Assert.AreEqual(success, dependency.Success.Value);
        }
    }
}
