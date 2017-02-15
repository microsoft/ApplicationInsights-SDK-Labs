using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
using Microsoft.ApplicationInsights.Wcf.Tests.Integration;
using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class ClientTelemetryRequestChannelTests
    {
        const String TwoWayOp1 = "http://tempuri.org/ISimpleService/GetSimpleData";
        const String TwoWayOp2 = "http://tempuri.org/ISimpleService/CallFailsWithFault";
        const String OneWayOp1 = "http://tempuri.org/IOneWayService/SuccessfullOneWayCall";

        const String HostName = "localhost";
        const String SvcUrl = "http://localhost/MyService.svc";

        private ClientTelemetryRequestChannel GetChannel(IChannel innerChannel, Type contract)
        {
            return new ClientTelemetryRequestChannel(
                new ClientChannelManager(new TelemetryClient(), contract, BuildOperationMap()),
                innerChannel
                );
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelManagerIsNull_ConstructorThrowsException()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            bool failed = false;
            try
            {
                var channel = new ClientTelemetryRequestChannel(null, innerChannel);
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

            var manager = new ClientChannelManager(new TelemetryClient(), typeof(ISimpleService), BuildOperationMap()),
            bool failed = false;
            try
            {
                var channel = new ClientTelemetryRequestChannel(manager, null);
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
            Assert.AreEqual(innerChannel.RemoteAddress, channel.RemoteAddress);
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

            CheckDependencyWritten(DependencyConstants.WcfChannelOpen, typeof(ISimpleService), true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenChannelIsOpenedWithTimeout_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));
            channel.Open(TimeSpan.FromSeconds(10));

            CheckDependencyWritten(DependencyConstants.WcfChannelOpen, typeof(ISimpleService), true);
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

            CheckDependencyWritten(DependencyConstants.WcfChannelOpen, typeof(ISimpleService), true);
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

            CheckDependencyWritten(DependencyConstants.WcfChannelOpen, typeof(ISimpleService), true);
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

            CheckDependencyWritten(DependencyConstants.WcfChannelOpen, typeof(ISimpleService), false);
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

            CheckDependencyWritten(DependencyConstants.WcfChannelOpen, typeof(ISimpleService), false);
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

            CheckDependencyWritten(DependencyConstants.WcfChannelOpen, typeof(ISimpleService), false);
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

            CheckDependencyWritten(DependencyConstants.WcfChannelOpen, typeof(ISimpleService), false);
        }



        //
        // Request Telemetry
        //
        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            var response = channel.Request(BuildMessage(TwoWayOp1));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSentWithTimeout_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            var response = channel.Request(BuildMessage(TwoWayOp1), TimeSpan.FromSeconds(10));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_TelemetryIsWritten_SoapFault()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.ReturnSoapFault = true;

            var response = channel.Request(BuildMessage(TwoWayOp1), TimeSpan.FromSeconds(10));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_TelemetryIsWritten_Exception()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailRequest = true;

            bool failed = false;
            try
            {
                var response = channel.Request(BuildMessage(TwoWayOp1));
            } catch {
                failed = true;
            }
            Assert.IsTrue(failed, "Request did not throw an exception");
            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSentWithTimeout_TelemetryIsWritten_Exception()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailRequest = true;

            bool failed = false;
            try
            {
                var response = channel.Request(BuildMessage(TwoWayOp1), TimeSpan.FromSeconds(10));
            } catch {
                failed = true;
            }
            Assert.IsTrue(failed, "Request did not throw an exception");
            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_Async_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            var result = channel.BeginRequest(BuildMessage(TwoWayOp1), null, null);
            var response = channel.EndRequest(result);

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_AsyncWithTimeout_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            var result = channel.BeginRequest(BuildMessage(TwoWayOp1), TimeSpan.FromSeconds(10), null, null);
            var response = channel.EndRequest(result);

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_Async_TelemetryIsWritten_Exception()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailRequest = true;

            var failed = false;
            try
            {
                var result = channel.BeginRequest(BuildMessage(TwoWayOp1), null, null);

            } catch
            {
                failed = true;
            }
            Assert.IsTrue(failed, "BeginRequest did not throw an exception");

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_AsyncWithTimeout_TelemetryIsWritten_Exception()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailRequest = true;

            var failed = false;
            try
            {
                var result = channel.BeginRequest(BuildMessage(TwoWayOp1), TimeSpan.FromSeconds(10), null, null);

            } catch
            {
                failed = true;
            }
            Assert.IsTrue(failed, "BeginRequest did not throw an exception");

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_Async_TelemetryIsWritten_ExceptionOnEnd()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailEndRequest = true;

            var result = channel.BeginRequest(BuildMessage(TwoWayOp1), null, null);
            var failed = false;
            try
            {
                var response = channel.EndRequest(result);
            } catch
            {
                failed = true;
            }
            Assert.IsTrue(failed, "EndRequest did not throw an exception");

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", false);
        }



        private Message BuildMessage(String action)
        {
            return Message.CreateMessage(MessageVersion.Default, action, "<text/>");
        }

        private void CheckDependencyWritten(String type, Type contract, bool success)
        {
            var dependency = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().FirstOrDefault();
            Assert.IsNotNull(dependency, "Did not write dependency event");
            Assert.AreEqual(SvcUrl, dependency.Name);
            Assert.AreEqual("localhost", dependency.Target);
            Assert.AreEqual(type, dependency.Type);
            Assert.AreEqual(contract.FullName, dependency.Data);
            Assert.AreEqual(success, dependency.Success.Value);
        }
        private void CheckOpDependencyWritten(String type, Type contract, String action, String method, bool success)
        {
            var dependency = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().FirstOrDefault();
            Assert.IsNotNull(dependency, "Did not write dependency event");
            Assert.AreEqual(SvcUrl, dependency.Name);
            Assert.AreEqual("localhost", dependency.Target);
            Assert.AreEqual(type, dependency.Type);
            Assert.AreEqual(contract.Name + "." + method, dependency.Data);
            Assert.AreEqual(action, dependency.Properties["soapAction"]);
            Assert.AreEqual(success, dependency.Success.Value);
        }

        private ClientOperationMap BuildOperationMap()
        {
            ClientOpDescription[] ops = new ClientOpDescription[]
            {
                new ClientOpDescription { Action = TwoWayOp1, IsOneWay = false, Name = "GetSimpleData" },
                new ClientOpDescription { Action = TwoWayOp2, IsOneWay = false, Name = "CallFailsWithFault" },
                new ClientOpDescription { Action = OneWayOp1, IsOneWay = true, Name = "SuccessfullOneWayCall" },
            };
            return new ClientOperationMap(ops);
        }
    }
}
