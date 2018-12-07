namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OperationNameTelemetryInitializerTests
    {
        [TestMethod]
        public void NonHttpEndpoint()
        {
            var context = new MockOperationContext();
            context.EndpointUri = new Uri("net.tcp://localhost/Service1.svc");
            context.OperationName = "GetData";

            var initializer = new OperationNameTelemetryInitializer();
            var telemetry = new RequestTelemetry();
            initializer.Initialize(telemetry, context);

            string name = telemetry.Context.Operation.Name;
            Assert.AreEqual("IFakeService.GetData", name);
        }

        [TestMethod]
        public void RequestNameEqualsOperationName()
        {
            var context = new MockOperationContext();
            context.EndpointUri = new Uri("net.tcp://localhost/Service1.svc");
            context.OperationName = "GetData";

            var initializer = new OperationNameTelemetryInitializer();
            var telemetry = new RequestTelemetry();
            initializer.Initialize(telemetry, context);

            string name = telemetry.Context.Operation.Name;
            Assert.AreEqual(name, telemetry.Name);
        }

        [TestMethod]
        public void HttpEndpointDoesNotHaveMethodInName()
        {
            var context = new MockOperationContext();
            context.EndpointUri = new Uri("http://localhost/Service1.svc");
            context.OperationName = "GetData";

            HttpRequestMessageProperty http = new HttpRequestMessageProperty();
            http.Method = "POST";
            context.IncomingProperties.Add(HttpRequestMessageProperty.Name, http);

            var initializer = new OperationNameTelemetryInitializer();
            var telemetry = new RequestTelemetry();
            initializer.Initialize(telemetry, context);

            string name = telemetry.Context.Operation.Name;
            Assert.AreEqual(name, "IFakeService.GetData");
        }

        [TestMethod]
        public void OperationNameIsCopiedFromRequestIfPresent()
        {
            const string Name = "MyOperationName";
            var context = new MockOperationContext();
            context.EndpointUri = new Uri("net.tcp://localhost/Service1.svc");
            context.OperationName = "GetData";

            context.Request.Context.Operation.Name = Name;

            var initializer = new OperationNameTelemetryInitializer();
            var telemetry = new EventTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(Name, telemetry.Context.Operation.Name);
        }
    }
}
