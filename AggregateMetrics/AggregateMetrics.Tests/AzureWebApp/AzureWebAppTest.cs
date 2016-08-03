namespace webAppTest
{
    using AggregateMetrics.Tests.AzureWebApp;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
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
            metric.Value = CacheHelperTests.Instance.GetCounterValue(performanceCounter);

            Assert.IsTrue(metric.Value >= 0);
        }
    }
}
