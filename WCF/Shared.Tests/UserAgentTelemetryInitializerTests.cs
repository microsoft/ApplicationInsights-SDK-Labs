using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
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
    }
}
