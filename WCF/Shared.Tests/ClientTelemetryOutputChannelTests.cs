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
    public class ClientTelemetryOutputChannelTests : ChannelTestBase<IOutputChannel>
    {
        private const string OneWayOp1 = "http://tempuri.org/IOneWayService/SuccessfullOneWayCall";

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSent_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(IOneWayService));

            channel.Send(BuildMessage(OneWayOp1));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSentWithTimeout_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(IOneWayService));

            channel.Send(BuildMessage(OneWayOp1), TimeSpan.FromSeconds(10));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSent_TelemetryIsWritten_Exception()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(IOneWayService));

            innerChannel.FailRequest = true;

            bool failed = false;
            try
            {
                channel.Send(BuildMessage(OneWayOp1));
            }
            catch
            {
                failed = true;
            }

            Assert.IsTrue(failed, "Send did not throw an exception");

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSent_WithTimeoutException_TelemetryHasResultCode()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(IOneWayService));

            innerChannel.FailRequest = true;
            innerChannel.ExceptionToThrowOnSend = new TimeoutException();

            bool failed = false;
            try
            {
                channel.Send(BuildMessage(OneWayOp1));
            }
            catch (TimeoutException)
            {
                failed = true;
            }

            Assert.IsTrue(failed, "Send did not throw TimeoutException");

            var telemetry = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().First();
            Assert.AreEqual("Timeout", telemetry.ResultCode);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSent_Async_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(IOneWayService));

            var result = channel.BeginSend(BuildMessage(OneWayOp1), null, null);
            channel.EndSend(result);

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSentWithTimeout_Async_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(IOneWayService));

            var result = channel.BeginSend(BuildMessage(OneWayOp1), TimeSpan.FromSeconds(10), null, null);
            channel.EndSend(result);

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSent_Async_TelemetryIsWritten_ExceptionOnEnd()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(IOneWayService));

            innerChannel.FailEndRequest = true;

            var result = channel.BeginSend(BuildMessage(OneWayOp1), TimeSpan.FromSeconds(10), null, null);
            bool failed = false;
            try
            {
                channel.EndSend(result);
            }
            catch
            {
                failed = true;
            }

            Assert.IsTrue(failed, "EndSend did not throw an exception");

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", false);
        }

        // -----------------------
        // Other tests
        // -----------------------
        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSent_CorrelationHeadersAreSet()
        {
            var client = new TelemetryClient();
            client.Context.Operation.Id = "12345"; // parentId

            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = this.GetChannel(innerChannel, typeof(IOneWayService), client);

            channel.Send(BuildMessage(OneWayOp1));

            var message = innerChannel.LastMessageSent;

            Assert.AreEqual(client.Context.Operation.Id, GetHttpHeader(message, CorrelationHeaders.HttpStandardRootIdHeader));
            Assert.IsNotNull(GetHttpHeader(message, CorrelationHeaders.HttpStandardParentIdHeader));
            Assert.AreEqual(client.Context.Operation.Id, GetSoapHeader(message, CorrelationHeaders.SoapStandardNamespace, CorrelationHeaders.SoapStandardRootIdHeader));
            Assert.IsNotNull(GetSoapHeader(message, CorrelationHeaders.SoapStandardNamespace, CorrelationHeaders.SoapStandardParentIdHeader));
        }

        internal override IOutputChannel GetChannel(IChannel innerChannel, Type contract)
        {
            return this.GetChannel(innerChannel, contract, null);
        }

        internal override IOutputChannel GetChannel(IChannelManager manager, IChannel innerChannel)
        {
            return new ClientTelemetryOutputChannel(manager, innerChannel);
        }

        private static string GetSoapHeader(Message message, string ns, string headerName)
        {
            return message.Headers.GetHeader<string>(headerName, ns);
        }

        private static string GetHttpHeader(Message message, string headerName)
        {
            var httpHeaders = message.GetHttpRequestHeaders();
            return httpHeaders.Headers[headerName];
        }

        private static Message BuildMessage(string action)
        {
            return Message.CreateMessage(MessageVersion.Default, action, "<text/>");
        }

        private static void CheckOpDependencyWritten(string type, Type contract, string action, string method, bool success)
        {
            var dependency = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().FirstOrDefault();
            Assert.IsNotNull(dependency, "Did not write dependency event");
            Assert.AreEqual(ClientTelemetryOutputChannelTests.SvcUrl, dependency.Data);
            Assert.AreEqual("localhost", dependency.Target);
            Assert.AreEqual(type, dependency.Type);
            Assert.AreEqual(contract.Name + "." + method, dependency.Name);
            Assert.AreEqual(action, dependency.Properties["soapAction"]);
            Assert.AreEqual("True", dependency.Properties["isOneWay"]);
            Assert.AreEqual(success, dependency.Success.Value);
        }

        private IOutputChannel GetChannel(IChannel innerChannel, Type contract, TelemetryClient client)
        {
            return this.GetChannel(
                new ClientChannelManager(client ?? new TelemetryClient(), contract),
                innerChannel);
        }
    }
}
