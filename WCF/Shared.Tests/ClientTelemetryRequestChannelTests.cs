namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.Linq;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Wcf.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
    using Microsoft.ApplicationInsights.Wcf.Tests.Integration;
    using Microsoft.ApplicationInsights.Wcf.Tests.Service;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClientTelemetryRequestChannelTests : ChannelTestBase<IRequestChannel>
    {
        private const string TwoWayOp1 = "http://tempuri.org/ISimpleService/GetSimpleData";
        private const string TwoWayOp2 = "http://tempuri.org/ISimpleService/CallFailsWithFault";
        private const string OneWayOp1 = "http://tempuri.org/IOneWayService/SuccessfullOneWayCall";

        // ----------------------------------
        // Request Telemetry
        // ----------------------------------
        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

            var response = channel.Request(BuildMessage(TwoWayOp1));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSentWithTimeout_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

            var response = channel.Request(BuildMessage(TwoWayOp1), TimeSpan.FromSeconds(10));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_TelemetryIsWritten_SoapFault()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.ReturnSoapFault = true;

            var response = channel.Request(BuildMessage(TwoWayOp1), TimeSpan.FromSeconds(10));

            var telemetry = CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", false);
            Assert.AreEqual("SoapFault", telemetry.ResultCode);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenRequestIsSent_TelemetryIsWritten_Exception()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailRequest = true;

            bool failed = false;
            try
            {
                var response = channel.Request(BuildMessage(TwoWayOp1));
            }
            catch
            {
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
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailRequest = true;

            bool failed = false;
            try
            {
                var response = channel.Request(BuildMessage(TwoWayOp1), TimeSpan.FromSeconds(10));
            }
            catch
            {
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
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

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
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

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
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailRequest = true;

            var failed = false;
            try
            {
                var result = channel.BeginRequest(BuildMessage(TwoWayOp1), null, null);
            }
            catch
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
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailRequest = true;

            var failed = false;
            try
            {
                var result = channel.BeginRequest(BuildMessage(TwoWayOp1), TimeSpan.FromSeconds(10), null, null);
            }
            catch
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
            var channel = this.GetChannel(innerChannel, typeof(ISimpleService));

            innerChannel.FailEndRequest = true;

            var result = channel.BeginRequest(BuildMessage(TwoWayOp1), null, null);
            var failed = false;
            try
            {
                var response = channel.EndRequest(result);
            }
            catch
            {
                failed = true;
            }

            Assert.IsTrue(failed, "EndRequest did not throw an exception");

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(ISimpleService), TwoWayOp1, "GetSimpleData", false);
        }

        internal override IRequestChannel GetChannel(IChannel innerChannel, Type contract)
        {
            return new ClientTelemetryRequestChannel(
                new ClientChannelManager(new TelemetryClient(), contract),
                innerChannel);
        }

        internal override IRequestChannel GetChannel(IChannelManager manager, IChannel innerChannel)
        {
            return new ClientTelemetryRequestChannel(manager, innerChannel);
        }

        private static Message BuildMessage(string action)
        {
            return Message.CreateMessage(MessageVersion.Default, action, "<text/>");
        }

        private static DependencyTelemetry CheckOpDependencyWritten(string type, Type contract, string action, string method, bool success)
        {
            var dependency = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().FirstOrDefault();
            Assert.IsNotNull(dependency, "Did not write dependency event");
            Assert.AreEqual(ClientTelemetryRequestChannelTests.SvcUrl, dependency.Data);
            Assert.AreEqual("localhost", dependency.Target);
            Assert.AreEqual(type, dependency.Type);
            Assert.AreEqual(contract.Name + "." + method, dependency.Name);
            Assert.AreEqual(action, dependency.Properties["soapAction"]);
            Assert.AreEqual(success, dependency.Success.Value);
            return dependency;
        }
    }
}
