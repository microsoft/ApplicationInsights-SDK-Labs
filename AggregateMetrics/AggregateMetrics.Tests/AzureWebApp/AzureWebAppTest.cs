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


            List<MetricTelemetry> metrics = DefaultCounters.Instance.GetCountersHttp();
            int countersFirstIteration = metrics.Count;

            foreach (MetricTelemetry metric in metrics)
            {
                Assert.IsTrue(metric.Value >= 0);
            }

            System.Threading.Thread.Sleep(7000);
            metrics = DefaultCounters.Instance.GetCountersHttp();
            int countersSecondIteration = metrics.Count;

            foreach (MetricTelemetry metric in metrics)
            {
                Assert.IsTrue(metric.Value >= 0);
            }

            Assert.IsTrue(countersSecondIteration > countersFirstIteration);
        }
    }
}
