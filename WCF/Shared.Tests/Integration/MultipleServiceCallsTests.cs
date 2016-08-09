using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    [TestClass]
    public class MultipleServiceCallsTests
    {
        [TestMethod]
        [TestCategory("Integration"), TestCategory("Sync")]
        public void OperationMethodThatCallsAnotherServiceDoesNotLoseOperationContext()
        {
            TestTelemetryChannel.Clear();
            using ( var host = new HostingContext<SimpleService, ISimpleService>() )
            using ( var hostSecond = new HostingContext<SimpleService, ISimpleService>() )
            {
                host.IncludeDetailsInFaults();
                host.Open();
                hostSecond.Open();
                ISimpleService client = host.GetChannel();
                client.CallAnotherServiceAndLeakOperationContext(hostSecond.GetServiceAddress());
                Assert.IsTrue(TestTelemetryChannel.CollectedData().Count > 0);
            }
        }

    }
}
