using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.ServiceModel.Description;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class OperationFilterTests
    {
        [TestMethod]
        public void WhenNoOpsAreInstrumentedShouldProcessReturnsTrue()
        {
            ContractDescription contract = ContractBuilder.CreateDescription(
                typeof(ISimpleService), typeof(SimpleService)
                );

            var filter = new OperationFilter(contract);
            foreach ( var operation in contract.Operations )
            {
                Assert.IsTrue(
                    filter.ShouldProcess(operation.Name),
                    "Operation {0} not processed", operation.Name
                );
            }
        }

        [TestMethod]
        public void WhenAnOpIsInstrumentedShouldProcessReturnsTrue()
        {
            ContractDescription contract = ContractBuilder.CreateDescription(
                typeof(ISelectiveTelemetryService), typeof(SelectiveTelemetryService)
                );

            var filter = new OperationFilter(contract);
            Assert.IsTrue(
                filter.ShouldProcess("OperationWithTelemetry"),
                "ShouldProcessRequest('OperationWithTelemetry') returned false"
            );
        }

        [TestMethod]
        public void WhenAnOpIsNotBeenInstrumentedShouldProcessReturnsFalse()
        {
            ContractDescription contract = ContractBuilder.CreateDescription(
                typeof(ISelectiveTelemetryService), typeof(SelectiveTelemetryService)
                );

            var filter = new OperationFilter(contract);
            Assert.IsFalse(
                filter.ShouldProcess("OperationWithoutTelemetry"),
                "ShouldProcessRequest('OperationWithoutTelemetry') returned true"
            );
        }

    }
}
