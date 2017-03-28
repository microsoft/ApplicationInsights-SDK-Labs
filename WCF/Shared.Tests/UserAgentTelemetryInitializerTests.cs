namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UserAgentTelemetryInitializerTests
    {
        [TestMethod]
        public void ContextUserAgentIsSetIfPresent()
        {
            var context = new MockOperationContext();
            context.EndpointUri = new Uri("http://localhost/Service1.svc");
            context.OperationName = "GetData";

            HttpRequestMessageProperty http = new HttpRequestMessageProperty();
            http.Headers["User-Agent"] = "MyUserAgent";
            context.IncomingProperties.Add(HttpRequestMessageProperty.Name, http);

            var initializer = new UserAgentTelemetryInitializer();
            var telemetry = new RequestTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.AreEqual("MyUserAgent", telemetry.Context.User.UserAgent);
        }

        [TestMethod]
        public void UserAgentIsCopiedFromRequestIfPresent()
        {
            var context = new MockOperationContext();
            context.EndpointUri = new Uri("http://localhost/Service1.svc");
            context.OperationName = "GetData";
            context.Request.Context.User.UserAgent = "MyUserAgent";

            var initializer = new UserAgentTelemetryInitializer();
            var telemetry = new EventTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(context.Request.Context.User.UserAgent, telemetry.Context.User.UserAgent);
        }
    }
}
