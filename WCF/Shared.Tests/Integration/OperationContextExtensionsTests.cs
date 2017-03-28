namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    using System;
    using System.ServiceModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OperationContextExtensionsTests
    {
        [ServiceContract]
        public interface IContextCheckService
        {
            [OperationContract]
            bool CanGetRequestTelemetry();
        }

        [TestMethod]
        public void WhenOperationContextIsNullReturnsNull()
        {
            var result = OperationContextExtensions.GetRequestTelemetry(null);
            Assert.IsNull(result);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void WhenOperationContextIsClientContextReturnsNull()
        {
            using (var host = new HostingContext<ContextCheckService, IContextCheckService>())
            {
                host.Open();
                var client = host.GetChannel();
                using (var scope = new OperationContextScope((IContextChannel)client))
                {
                    Assert.IsNull(OperationContext.Current.GetRequestTelemetry());
                }
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void WhenOperationContextIsServiceContextReturnsRequest()
        {
            using (var host = new HostingContext<ContextCheckService, IContextCheckService>())
            {
                host.Open();
                var client = host.GetChannel();
                using (var scope = new OperationContextScope((IContextChannel)client))
                {
                    Assert.IsTrue(client.CanGetRequestTelemetry());
                }
            }
        }

        [ServiceTelemetry]
        internal class ContextCheckService : IContextCheckService
        {
            public bool CanGetRequestTelemetry()
            {
                return OperationContext.Current.GetRequestTelemetry() != null;
            }
        }
    }
}
