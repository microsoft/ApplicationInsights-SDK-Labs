namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OperationCorrelationTelemetryInitializerTests
    {
        [TestMethod]
        public void SetsOperationIdWhenNoParentOperationIsPresent()
        {
            var context = new MockOperationContext();
            var initializer = new OperationCorrelationTelemetryInitializer();

            var telemetry = context.Request;
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(telemetry.Id, telemetry.Context.Operation.Id);
        }

        [TestMethod]
        public void InitializeDoesNotOverrideCustomerParentOperationId()
        {
            var context = new MockOperationContext();
            var initializer = new OperationCorrelationTelemetryInitializer();
            var telemetry = new TraceTelemetry();
            telemetry.Context.Operation.ParentId = "someId";
            initializer.Initialize(telemetry, context);

            Assert.AreEqual("someId", telemetry.Context.Operation.ParentId);
        }

        [TestMethod]
        public void InitializeSetsRootOperationIdForTelemetryUsingIdFromRequestTelemetry()
        {
            var context = new MockOperationContext();
            context.Request.Context.Operation.Id = "RootId";

            var initializer = new OperationCorrelationTelemetryInitializer();
            var exceptionTelemetry = new ExceptionTelemetry();

            initializer.Initialize(exceptionTelemetry, context);

            Assert.AreEqual(context.Request.Context.Operation.Id, exceptionTelemetry.Context.Operation.Id);
        }

        [TestMethod]
        public void InitializeDoesNotOverrideCustomerRootOperationId()
        {
            var context = new MockOperationContext();
            var initializer = new OperationCorrelationTelemetryInitializer();
            var requestTelemetry = context.Request;
            requestTelemetry.Context.Operation.Id = "RootId";

            var customerTelemetry = new TraceTelemetry();
            customerTelemetry.Context.Operation.Id = "CustomId";

            initializer.Initialize(customerTelemetry, context);

            Assert.AreEqual("CustomId", customerTelemetry.Context.Operation.Id);
        }

        [TestMethod]
        public void InitializeSetsRequestTelemetryRootOperationIdToOperationId()
        {
            var context = new MockOperationContext();
            var initializer = new OperationCorrelationTelemetryInitializer();
            var requestTelemetry = context.Request;

            var customerTelemetry = new TraceTelemetry();

            initializer.Initialize(customerTelemetry, context);

            Assert.AreEqual(requestTelemetry.Id, requestTelemetry.Context.Operation.Id);
        }

        [TestMethod]
        public void InitializeReadsRootIdFromCustomHeader()
        {
            var httpHeaders = new HttpRequestMessageProperty();
            httpHeaders.Headers["headerName"] = "RootId";

            var context = new MockOperationContext();
            context.SetHttpHeaders(httpHeaders);
            var initializer = new OperationCorrelationTelemetryInitializer();
            initializer.RootOperationIdHeaderName = "headerName";

            var requestTelemetry = context.Request;

            var customerTelemetry = new TraceTelemetry();

            initializer.Initialize(customerTelemetry, context);
            Assert.AreEqual("RootId", customerTelemetry.Context.Operation.Id);
            Assert.AreEqual("RootId", requestTelemetry.Context.Operation.Id);
        }

        [TestMethod]
        public void InitializeReadsRootIdFromCustomSoapHeader()
        {
            var context = new MockOperationContext();
            context.AddIncomingMessageHeader("headerName", "somenamespace", "RootId");

            var initializer = new OperationCorrelationTelemetryInitializer();
            initializer.SoapRootOperationIdHeaderName = "headerName";
            initializer.SoapHeaderNamespace = "somenamespace";

            var requestTelemetry = context.Request;

            var customerTelemetry = new TraceTelemetry();

            initializer.Initialize(customerTelemetry, context);
            Assert.AreEqual("RootId", customerTelemetry.Context.Operation.Id);
            Assert.AreEqual("RootId", requestTelemetry.Context.Operation.Id);
        }

        [TestMethod]
        public void InitializeDoNotMakeRequestAParentOfItself()
        {
            var context = new MockOperationContext();
            var initializer = new OperationCorrelationTelemetryInitializer();
            var requestTelemetry = context.Request;

            initializer.Initialize(requestTelemetry, context);
            Assert.AreEqual(null, requestTelemetry.Context.Operation.ParentId);
            Assert.AreEqual(requestTelemetry.Id, requestTelemetry.Context.Operation.Id);
        }
    }
}
