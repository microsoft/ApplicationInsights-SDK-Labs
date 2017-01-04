using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class ClientIpTelemetryInitializerTests
    {
        [TestMethod]
        public void SetsClientIpFromWcfContextOnRequest()
        {
            const String clientIp = "10.12.32.12";
            var context = new MockOperationContext();
            context.IncomingProperties.Add(
                RemoteEndpointMessageProperty.Name,
                new RemoteEndpointMessageProperty(clientIp, 7656));

            var initializer = new ClientIpTelemetryInitializer();

            var telemetry = context.Request;
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(clientIp, telemetry.Context.Location.Ip);
        }

        [TestMethod]
        public void SetsClientIpFromWcfContextOnOtherEvent()
        {
            const String originalIp = "10.12.32.12";
            const String newIp = "172.34.12.45";
            var context = new MockOperationContext();
            context.IncomingProperties.Add(
                RemoteEndpointMessageProperty.Name,
                new RemoteEndpointMessageProperty(originalIp, 7656));

            var initializer = new ClientIpTelemetryInitializer();

            // initialize request with the original IP
            initializer.Initialize(context.Request, context);

            // replace IP so that we can tell
            // it is being picked up from the request
            // rather than the context
            context.IncomingProperties.Clear();
            context.IncomingProperties.Add(
                RemoteEndpointMessageProperty.Name,
                new RemoteEndpointMessageProperty(newIp, 7656));

            var telemetry = new EventTelemetry("myevent");
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(originalIp, telemetry.Context.Location.Ip);
        }

        [TestMethod]
        public void ClientIpIsCopiedFromRequestIfPresent()
        {
            const String clientIp = "10.12.32.12";
            var context = new MockOperationContext();
            context.Request.Context.Location.Ip = clientIp;

            var initializer = new ClientIpTelemetryInitializer();
            var telemetry = new EventTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(clientIp, telemetry.Context.Location.Ip);
        }
    }
}
