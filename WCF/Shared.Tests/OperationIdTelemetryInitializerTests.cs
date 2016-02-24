using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class OperationIdTelemetryInitializerTests
    {
        [TestMethod]
        public void SetsOperationId()
        {
            var context = new MockOperationContext();
            var initializer = new OperationIdTelemetryInitializer();
            var telemetry = new RequestTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.IsNotNull(telemetry.Context.Operation.Id);
        }

        [TestMethod]
        public void SetsRequestTelemetryIdEqualsToOperationId()
        {
            var context = new MockOperationContext();
            var initializer = new OperationIdTelemetryInitializer();
            var telemetry = new RequestTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(telemetry.Context.Operation.Id, telemetry.Id);
        }

        [TestMethod]
        public void StoresOperationIdInWcfContext()
        {
            var context = new MockOperationContext();
            var initializer = new OperationIdTelemetryInitializer();
            var telemetry = new RequestTelemetry();
            initializer.Initialize(telemetry, context);

            Assert.AreEqual(telemetry.Context.Operation.Id, context.OperationId);
        }
    }
}
