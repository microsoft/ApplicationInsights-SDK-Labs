namespace AggregateMetrics.Tests.Two
{
    using Microsoft.ApplicationInsights;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Channel;
    using System.Threading;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

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
            module.FlushInterval = TimeSpan.FromSeconds(6);

            module.Initialize(config);

            var client = new TelemetryClient(config);
            client.Gauge("test", () => { return 10; });

            Thread.Sleep(TimeSpan.FromSeconds(7));

            Assert.AreEqual(1, sentItems.Count);
            var metric = (MetricTelemetry)sentItems[0];
            Assert.AreEqual("test", metric.Name);
            Assert.AreEqual(10, metric.Value);
        }

        [TestMethod]
        public void ModuleWillKeepIntervalWithThreadsStarvation()
        {
            var sentItems = new List<ITelemetry>();
            var channel = new StubTelemetryChannel() { OnSend = (item) => { sentItems.Add(item); } };
            var config = new TelemetryConfiguration();
            config.TelemetryChannel = channel;
            config.InstrumentationKey = "dummy";

            AggregateMetricsTelemetryModule module = new AggregateMetricsTelemetryModule();
            module.FlushInterval = TimeSpan.FromSeconds(6);

            module.Initialize(config);

            var startTime = DateTime.Now;

            var client = new TelemetryClient(config);
            client.Gauge("test", () => { return 10; });

            int workerThread;
            int ioCompletionThread;
            ThreadPool.GetMaxThreads(out workerThread, out ioCompletionThread);
            try
            {
                ThreadPool.SetMaxThreads(10, 10);
                for (int i = 0; i < 50; i++)
                {
                    new Task(() => {
                        Debug.WriteLine("task started");
                        Thread.Sleep(TimeSpan.FromSeconds(10));
                        Debug.WriteLine("task finihed");
                    }).Start();
                }

                Thread.Sleep(TimeSpan.FromSeconds(7));

                Assert.AreEqual(1, sentItems.Count);
                var metric = (MetricTelemetry)sentItems[0];
                Assert.AreEqual("test", metric.Name);
                Assert.AreEqual(10, metric.Value);
                Assert.IsTrue(metric.Timestamp.Subtract(startTime).Seconds <= 6, "Actual: " + metric.Timestamp.Subtract(startTime).Seconds);
                Assert.IsTrue(metric.Timestamp.Subtract(startTime).Seconds >= 5, "Actual: " + metric.Timestamp.Subtract(startTime).Seconds);
            }
            finally
            {
                ThreadPool.SetMaxThreads(workerThread, ioCompletionThread);
            }
        }

    }
}