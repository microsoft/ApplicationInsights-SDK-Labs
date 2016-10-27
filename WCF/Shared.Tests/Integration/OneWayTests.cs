using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    [TestClass]
    public class OneWayTests
    {
        [TestMethod]
        [TestCategory("Integration"), TestCategory("One-Way")]
        public void SuccessfulOneWayCallGeneratesRequestEvent()
        {
            TestTelemetryChannel.Clear();
            using ( var host = new HostingContext<OneWayService, IOneWayService>() )
            {
                host.Open();
                IOneWayService client = host.GetChannel();
                client.SuccessfullOneWayCall();
            }
            var req = TestTelemetryChannel.CollectedData()
                     .FirstOrDefault(x => x is RequestTelemetry);

            Assert.IsNotNull(req);
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("One-Way")]
        public void FailedOneWayCallGeneratesExceptionEvent()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<OneWayService, IOneWayService>()
                      .ExpectFailure().ShouldWaitForCompletion();
            using ( host )
            {
                host.Open();
                IOneWayService client = host.GetChannel();
                try
                {
                    client.FailureOneWayCall();
                } catch
                {
                }
            }
            var req = TestTelemetryChannel.CollectedData()
                     .FirstOrDefault(x => x is ExceptionTelemetry);

            Assert.IsNotNull(req);
        }
    }
}
