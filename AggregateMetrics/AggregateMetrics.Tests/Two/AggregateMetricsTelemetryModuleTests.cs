namespace AggregateMetrics.Tests.Two
{
    using Microsoft.ApplicationInsights;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.Channel;
    using System.Threading;
    using System;

    [TestClass]
    public class AggregateMetricsTelemetryModuleTests
    {

        [TestMethod]
        public void SimpleModuleUsage()
        {
            var sentItems = new List<ITelemetry>();
            var channel = new StubTelemetryChannel() { OnSend = (item) => { sentItems.Add(item); } };
            var config = new TelemetryConfiguration();
            config.TelemetryChannel = channel;
            config.InstrumentationKey = "dummy";

            AggregateMetricsTelemetryModule module = new AggregateMetricsTelemetryModule();
            module.Initialize(config);

            var client = new TelemetryClient(config);
            client.Gauge("test", () => { return 10; });

            Thread.Sleep(TimeSpan.FromSeconds(65));

            Assert.AreEqual(1, sentItems.Count);
            var metric = (MetricTelemetry)sentItems[0];
            Assert.AreEqual("test", metric.Name);
            Assert.AreEqual(10, metric.Value);
        }
    }
}