namespace webAppTest
{
    using System;
    using System.Diagnostics;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Channel;
    using System.Collections.Generic;
    using AggregateMetrics.Tests;
    using Microsoft.ApplicationInsights.Extensibility;
    [TestClass]
    public class UnitTestAzureWeb
    {
        [TestMethod]
        public void TestPerformanceCounterValuesAreCorrectlyRetrievedUsingFlexiblePerformanceCounterGauge()
        {
            string performanceCounter = "privateBytes";

            FlexiblePerformanceCounterGauge gauge = new FlexiblePerformanceCounterGauge(performanceCounter);

            MetricTelemetry metric = new MetricTelemetry();

            metric.Name = performanceCounter;
            metric.Value = CacheHelper.Instance.GetCounterValueHttp(performanceCounter);

            Assert.IsTrue(metric.Value >= 0);
        }

        [TestMethod]
        public void TestGetDefaultCountersWorks()
        {
            DefaultCounters defaultCounters = new DefaultCounters();
            defaultCounters.Initialize();

            List<MetricTelemetry> metrics = defaultCounters.GetCounters();

            Assert.AreEqual(3, metrics.Count);

            foreach (MetricTelemetry metric in metrics)
            {
                Assert.IsTrue(metric.Value >= 0);
            }

            System.Threading.Thread.Sleep(7000);
            metrics = defaultCounters.GetCounters();

            foreach (MetricTelemetry metric in metrics)
            {
                Assert.IsTrue(metric.Value >= 0);
            }
        }
    }
}
