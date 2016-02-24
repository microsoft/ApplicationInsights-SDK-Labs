using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ServiceModel.Description;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class ContractFilterTests
    {
        [TestMethod]
        public void WhenNoOpsInAnyContractAreInstrumentedShouldProcessReturnsTrue()
        {
            ContractDescription contract1 = ContractBuilder.CreateDescription(
                typeof(ISimpleService), typeof(SimpleService)
                );
            ContractDescription contract2 = ContractBuilder.CreateDescription(
                typeof(ISimpleService2), typeof(SimpleService)
                );

            ContractFilter filter = new ContractFilter(new[] { contract1, contract2 });
            foreach ( var operation in contract1.Operations )
            {
                Assert.IsTrue(
                    filter.ShouldProcess(contract1.Name, contract1.Namespace, operation.Name),
                    "Operation {0} not processed", operation.Name
                );
            }
            foreach ( var operation in contract2.Operations )
            {
                Assert.IsTrue(
                    filter.ShouldProcess(contract2.Name, contract2.Namespace, operation.Name),
                    "Operation {0} not processed", operation.Name
                );
            }
        }

        [TestMethod]
        public void WhenOpsInOneContractAreInstrumentedShouldProcessReturnsTrueForAllInstrumented()
        {
            ContractDescription contract1 = ContractBuilder.CreateDescription(
                typeof(ISimpleService), typeof(SimpleService)
                );
            ContractDescription contract2 = ContractBuilder.CreateDescription(
                typeof(ISelectiveTelemetryService), typeof(SelectiveTelemetryService)
                );

            // Only check contract 1 operations
            ContractFilter filter = new ContractFilter(new[] { contract1, contract2 });
            foreach ( var operation in contract1.Operations )
            {
                Assert.IsTrue(
                    filter.ShouldProcess(contract1.Name, contract1.Namespace, operation.Name),
                    "Operation {0} not processed", operation.Name
                );
            }

            // Check instrumented operation in second contract
            Assert.IsTrue(
                filter.ShouldProcess(contract2.Name, contract2.Namespace, "OperationWithTelemetry"),
                "Operation {0} not processed", "OperationWithTelemetry"
            );

        }

        [TestMethod]
        public void WhenOpsInOneContractAreInstrumentedShouldProcessReturnsFalseForAllInstrumented()
        {
            ContractDescription contract1 = ContractBuilder.CreateDescription(
                typeof(ISimpleService), typeof(SimpleService)
                );
            ContractDescription contract2 = ContractBuilder.CreateDescription(
                typeof(ISelectiveTelemetryService), typeof(SelectiveTelemetryService));

            ContractFilter filter = new ContractFilter(new[] { contract1, contract2 });
            Assert.IsFalse(
                filter.ShouldProcess(contract2.Name, contract2.Namespace, "OperationWithoutTelemetry"),
                "Operation {0} is processed", "OperationWithoutTelemetry"
            );
        }
    }
}
