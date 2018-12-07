namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    using System;
    using System.Linq;
    using System.Xml;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
    using Microsoft.ApplicationInsights.Wcf.Tests.Service;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TracingTests
    {
        [TestMethod]
        [TestCategory("Integration"), TestCategory("MessageTracing")]
        public void RequestIsTraced()
        {
            TraceTelemetryModule.Enable();
            try
            {
                TestTelemetryChannel.Clear();
                using (var host = new HostingContext<SimpleService, ISimpleService>())
                {
                    host.Open();
                    ISimpleService client = host.GetChannel();
                    client.GetSimpleData();
                }

                var trace = TestTelemetryChannel.CollectedData()
                    .OfType<EventTelemetry>()
                    .FirstOrDefault(x => x.Name == "WcfRequest");
                Assert.IsNotNull(trace, "No WcfRequest trace found");
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(trace.Properties["Body"]);
            }
            finally
            {
                TraceTelemetryModule.Disable();
            }
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("MessageTracing")]
        public void ResponseIsTraced()
        {
            TraceTelemetryModule.Enable();
            try
            {
                TestTelemetryChannel.Clear();
                using (var host = new HostingContext<SimpleService, ISimpleService>())
                {
                    host.Open();
                    ISimpleService client = host.GetChannel();
                    client.GetSimpleData();
                }

                var trace = TestTelemetryChannel.CollectedData()
                    .OfType<EventTelemetry>()
                    .FirstOrDefault(x => x.Name == "WcfResponse");
                Assert.IsNotNull(trace, "No WcfResponse trace found");
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(trace.Properties["Body"]);
            }
            finally
            {
                TraceTelemetryModule.Disable();
            }
        }
    }
}
