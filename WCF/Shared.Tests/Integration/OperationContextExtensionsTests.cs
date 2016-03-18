using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    [TestClass]
    public class OperationContextExtensionsTests
    {
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
            using ( var host = new HostingContext<ContextCheckService, IContextCheckService>() )
            {
                host.Open();
                var client = host.GetChannel();
                using ( var scope = new OperationContextScope((IContextChannel)client) )
                {
                    Assert.IsNull(OperationContext.Current.GetRequestTelemetry());
                }
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void WhenOperationContextIsServiceContextReturnsRequest()
        {
            using ( var host = new HostingContext<ContextCheckService, IContextCheckService>() )
            {
                host.Open();
                var client = host.GetChannel();
                using ( var scope = new OperationContextScope((IContextChannel)client) )
                {
                    Assert.IsTrue(client.CanGetRequestTelemetry());
                }
            }
        }


        [ServiceContract]
        public interface IContextCheckService
        {
            [OperationContract]
            bool CanGetRequestTelemetry();
        }

        [ServiceTelemetry]
        class ContextCheckService : IContextCheckService
        {
            public bool CanGetRequestTelemetry()
            {
                return OperationContext.Current.GetRequestTelemetry() != null;
            }
        }
    }
}
