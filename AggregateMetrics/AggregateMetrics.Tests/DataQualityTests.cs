namespace AggregateMetrics.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DataQualityTests
    {
        [TestInitialize]
        public void Initialize()
        {
            AggregateMetricsTelemetryModule.FlushIntervalSeconds = 5;
            AggregateMetricsTelemetryModule.IsTimerFlushEnabled = true;
            AggregateMetrics.Clear();
        }

        [TestCleanup]
        public void Cleanup()
        {
            AggregateMetrics.Clear();
        }

        [TestMethod]
        public void SimpleTrackMetricCompare()
        {
            var channel = new StubTelemetryChannel();
            var client = GetTestTelemetryClient(channel);

            bool stopSending = false;

            var delayTask = Task.Delay(10000).ContinueWith((t) =>
            {
                stopSending = true;
            });

            while (!stopSending)
            {
                double val = new Random().NextDouble() * 100;
                client.TrackMetric("simple", val);
                client.TrackAggregateMetric("agg", val);

                Thread.Sleep((int)val);
            }

            delayTask.Wait();
            AggregateMetrics.Flush();

            channel.AssertSingleAndAggMetricsAreEqual();
        }

        [TestMethod]
        public void MetricOnePropCompare()
        {
            var channel = new StubTelemetryChannel();
            var client = GetTestTelemetryClient(channel);

            bool stopSending = false;

            var delayTask = Task.Delay(10000).ContinueWith((t) =>
            {
                stopSending = true;
            });

            while (!stopSending)
            {
                double val = new Random().NextDouble() * 100;
                string city = val > 50 ? "Seattle" : "New York";

                client.TrackMetric("simple", val);
                client.TrackAggregateMetric("agg", val, city);

                Thread.Sleep((int)val);
            }

            delayTask.Wait();
            AggregateMetrics.Flush();

            channel.AssertSingleAndAggMetricsAreEqual();
        }

        [TestMethod]
        public void MetricTwoPropsCompare()
        {
            var channel = new StubTelemetryChannel();
            var client = GetTestTelemetryClient(channel);

            bool stopSending = false;

            var delayTask = Task.Delay(10000).ContinueWith((t) =>
            {
                stopSending = true;
            });

            while (!stopSending)
            {
                double val = new Random().NextDouble() * 100;
                string city = val > 50 ? "Seattle" : "New York";
                string currency = ((int)val % 2) == 0 ? "Dollar" : "Pound";

                client.TrackMetric("simple", val);
                client.TrackAggregateMetric("agg", val, city, currency);

                Thread.Sleep((int)val);
            }

            delayTask.Wait();
            AggregateMetrics.Flush();

            channel.AssertSingleAndAggMetricsAreEqual();
        }

        [TestMethod]
        public void MetricThreePropsCompare()
        {
            var channel = new StubTelemetryChannel();
            var client = GetTestTelemetryClient(channel);

            bool stopSending = false;

            var delayTask = Task.Delay(10000).ContinueWith((t) =>
            {
                stopSending = true;
            });

            while (!stopSending)
            {
                double val = new Random().NextDouble() * 100;
                string city = val > 50 ? "Seattle" : "New York";
                string currency = ((int)val % 2) == 0 ? "Dollar" : "Pound";
                string animal = ((int)val % 2) == 0 ? "Bear" : "Drop Bear";

                client.TrackMetric("simple", val);
                client.TrackAggregateMetric("agg", val, city, currency, animal);

                Thread.Sleep((int)val);
            }

            delayTask.Wait();
            AggregateMetrics.Flush();

            channel.AssertSingleAndAggMetricsAreEqual();
        }

        protected class StubTelemetryChannel : ITelemetryChannel
        {
            public bool DeveloperMode { get; set; }
            public string EndpointAddress { get; set; }

            public IList<MetricTelemetry> MetricsSent { get; set; }

            public StubTelemetryChannel()
            {
                this.MetricsSent = new List<MetricTelemetry>();
            }

            public void AssertSingleAndAggMetricsAreEqual()
            {
                var simpleSet = this.MetricsSent.Where(i => i.Name == "simple");
                var aggSet = this.MetricsSent.Where(i => i.Name == "agg");

                double simpleSum = simpleSet.Sum(i => i.Value);
                double aggSum = aggSet.Sum(i => i.Value);

                // Sum of doubles is inherently imprecise.
                Assert.AreEqual(simpleSum, aggSum, 0.0000001);

                int simpleCount = simpleSet.Count();
                int aggCount = aggSet.Sum(i => i.Count.Value);

                Assert.AreEqual(simpleCount, aggCount);
            }

            public void Flush()
            {
            }

            public void Send(ITelemetry item)
            {
                if (item is MetricTelemetry)
                {
                    this.MetricsSent.Add(item as MetricTelemetry);
                }
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
            }
        }

        private static TelemetryClient GetTestTelemetryClient(ITelemetryChannel channel)
        {
            var config = new TelemetryConfiguration()
            {
                TelemetryChannel = channel,
                InstrumentationKey = "fake"
            };

            config.TelemetryModules.Add(new AggregateMetricsTelemetryModule());

            return new TelemetryClient(config);
        }
    }
}
