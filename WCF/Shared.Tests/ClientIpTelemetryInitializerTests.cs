namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClientIpTelemetryInitializerTests
    {
        [TestMethod]
        public void SetsClientIpFromWcfContextOnRequest()
        {
            const string ClientIp = "10.12.32.12";
            var context = new MockOperationContext();
            context.IncomingProperties.Add(
                RemoteEndpointMessageProperty.Name,
                new RemoteEndpointMessageProperty(ClientIp, 7656));

            var initializer = new ClientIpTelemetryInitializer();

            var telemetry = context.Request;
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(ClientIp, telemetry.Context.Location.Ip);
        }

        [TestMethod]
        public void SetsClientIpFromWcfContextOnOtherEvent()
        {
            const string OriginalIp = "10.12.32.12";
            const string NewIp = "172.34.12.45";
            var context = new MockOperationContext();
            context.IncomingProperties.Add(
                RemoteEndpointMessageProperty.Name,
                new RemoteEndpointMessageProperty(OriginalIp, 7656));

            var initializer = new ClientIpTelemetryInitializer();

            // initialize request with the original IP
            initializer.Initialize(context.Request, context);

            // replace IP so that we can tell
            // it is being picked up from the request
            // rather than the context
            context.IncomingProperties.Clear();
            context.IncomingProperties.Add(
                RemoteEndpointMessageProperty.Name,
                new RemoteEndpointMessageProperty(NewIp, 7656));

            var telemetry = new EventTelemetry("myevent");
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(OriginalIp, telemetry.Context.Location.Ip);
        }

        [TestMethod]
        public void ClientIpIsCopiedFromRequestIfPresent()
        {
            const string ClientIp = "10.12.32.12";
            var context = new MockOperationContext();
            context.Request.Context.Location.Ip = ClientIp;

            var initializer = new ClientIpTelemetryInitializer();
            var telemetry = new EventTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(ClientIp, telemetry.Context.Location.Ip);
        }
    }
}
