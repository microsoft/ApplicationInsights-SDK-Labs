using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
using Microsoft.ApplicationInsights.Wcf.Tests.Integration;
using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class ClientTelemetryOutputChannelTests : ChannelTestBase<IOutputChannel>
    {
        const String OneWayOp1 = "http://tempuri.org/IOneWayService/SuccessfullOneWayCall";

        private IOutputChannel GetChannel(IChannel innerChannel, Type contract, TelemetryClient client)
        {
            return GetChannel(
                new ClientChannelManager(client ?? new TelemetryClient(), contract),
                innerChannel
                );
        }
        internal override IOutputChannel GetChannel(IChannel innerChannel, Type contract)
        {
            return GetChannel(innerChannel, contract, null);
        }
        internal override IOutputChannel GetChannel(IChannelManager manager, IChannel innerChannel)
        {
            return new ClientTelemetryOutputChannel(manager, innerChannel);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSent_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(IOneWayService));

            channel.Send(BuildMessage(OneWayOp1));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSentWithTimeout_TelemetryIsWritten()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(IOneWayService));

            channel.Send(BuildMessage(OneWayOp1), TimeSpan.FromSeconds(10));

            CheckOpDependencyWritten(DependencyConstants.WcfClientCall, typeof(IOneWayService), OneWayOp1, "SuccessfullOneWayCall", true);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSent_TelemetryIsWritten_Exception()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(IOneWayService));

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
        [TestCategory("Client")]
        public void WhenMessageIsSent_WithTimeoutException_TelemetryHasResultCode()
        {
            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(IOneWayService));

            innerChannel.FailRequest = true;
            innerChannel.ExceptionToThrowOnSend = new TimeoutException();

            bool failed = false;
            try
            {
                channel.Send(BuildMessage(OneWayOp1));
            } catch ( TimeoutException )
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
            var channel = GetChannel(innerChannel, typeof(IOneWayService));

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
            var channel = GetChannel(innerChannel, typeof(IOneWayService));

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
            var channel = GetChannel(innerChannel, typeof(IOneWayService));

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


        //
        // Other tests
        //
        [TestMethod]
        [TestCategory("Client")]
        public void WhenMessageIsSent_CorrelationHeadersAreSet()
        {
            var client = new TelemetryClient();
            client.Context.Operation.Id = "12345"; // parentId

            var innerChannel = new MockClientChannel(SvcUrl);
            TestTelemetryChannel.Clear();
            var channel = GetChannel(innerChannel, typeof(IOneWayService), client);

            channel.Send(BuildMessage(OneWayOp1));

            var message = innerChannel.LastMessageSent;

            Assert.AreEqual(client.Context.Operation.Id, GetHttpHeader(message, CorrelationHeaders.HttpStandardRootIdHeader));
            Assert.IsNotNull(GetHttpHeader(message, CorrelationHeaders.HttpStandardParentIdHeader));
            Assert.AreEqual(client.Context.Operation.Id, GetSoapHeader(message, CorrelationHeaders.SoapStandardNamespace, CorrelationHeaders.SoapStandardRootIdHeader));
            Assert.IsNotNull(GetSoapHeader(message, CorrelationHeaders.SoapStandardNamespace, CorrelationHeaders.SoapStandardParentIdHeader));
        }

        private String GetSoapHeader(Message message, String ns, String headerName)
        {
            return message.Headers.GetHeader<String>(headerName, ns);
        }

        private String GetHttpHeader(Message message, String headerName)
        {
            var httpHeaders = message.GetHttpRequestHeaders();
            return httpHeaders.Headers[headerName];
        }

        private Message BuildMessage(String action)
        {
            return Message.CreateMessage(MessageVersion.Default, action, "<text/>");
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
