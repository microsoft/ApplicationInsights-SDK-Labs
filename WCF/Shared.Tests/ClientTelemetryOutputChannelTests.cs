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
    public class ClientTelemetryOutputChannelTests
    {
        const String OneWayOp1 = "http://tempuri.org/IOneWayService/SuccessfullOneWayCall";

        const String HostName = "localhost";
        const String SvcUrl = "http://localhost/MyService.svc";


        [TestMethod]
        public void WhenMessageIsSent_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new ClientTelemetryOutputChannel(client, innerChannel, typeof(IOneWayService), BuildOperationMap());

            channel.Send(BuildMessage(OneWayOp1));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        public void WhenMessageIsSentWithTimeout_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new ClientTelemetryOutputChannel(client, innerChannel, typeof(IOneWayService), BuildOperationMap());

            channel.Send(BuildMessage(OneWayOp1), TimeSpan.FromSeconds(10));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        public void WhenMessageIsSent_TelemetryIsWritten_Exception()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new ClientTelemetryOutputChannel(client, innerChannel, typeof(IOneWayService), BuildOperationMap());

            innerChannel.FailRequest = true;

            bool failed = false;
            try
            {
                channel.Send(BuildMessage(OneWayOp1));
            } catch
            {
                failed = true;
            }
            Assert.IsTrue(failed, "Send did not throw an exception");

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", false);
        }

        [TestMethod]
        public void WhenMessageIsSent_Async_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new ClientTelemetryOutputChannel(client, innerChannel, typeof(IOneWayService), BuildOperationMap());

            var result = channel.BeginSend(BuildMessage(OneWayOp1), null, null);
            channel.EndSend(result);

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        public void WhenMessageIsSentWithTimeout_Async_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new ClientTelemetryOutputChannel(client, innerChannel, typeof(IOneWayService), BuildOperationMap());

            var result = channel.BeginSend(BuildMessage(OneWayOp1), TimeSpan.FromSeconds(10), null, null);
            channel.EndSend(result);

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        public void WhenMessageIsSent_Async_TelemetryIsWritten_ExceptionOnEnd()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var channel = new ClientTelemetryOutputChannel(client, innerChannel, typeof(IOneWayService), BuildOperationMap());

            innerChannel.FailEndRequest = true;

            var result = channel.BeginSend(BuildMessage(OneWayOp1), TimeSpan.FromSeconds(10), null, null);
            bool failed = false;
            try
            {
                channel.EndSend(result);
            } catch
            {
                failed = true;
            }
            Assert.IsTrue(failed, "EndSend did not throw an exception");

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", false);
        }

        private Message BuildMessage(String action)
        {
            return Message.CreateMessage(MessageVersion.Default, action, "<text/>");
        }

        private ClientOperationMap BuildOperationMap()
        {
            ClientOpDescription[] ops = new ClientOpDescription[]
            {
                new ClientOpDescription { Action = OneWayOp1, IsOneWay = true, Name = "SuccessfullOneWayCall" },
            };
            return new ClientOperationMap(ops);
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
            Assert.AreEqual("True", dependency.Properties["isOneWay"]);
            Assert.AreEqual(success, dependency.Success.Value);
        }
    }
}
