namespace webAppTest
{
    using System;
    using System.Diagnostics;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.DataContracts;
    [TestClass]
    public class UnitTestAzureWeb
    {
        [TestMethod]
        public void GetCounterValue()
        {
            string performanceCounter = "privateBytes";

            FlexiblePerformanceCounterGauge gauge = new FlexiblePerformanceCounterGauge(performanceCounter);

            var metric = new MetricTelemetry();

            metric.Name = performanceCounter;
            metric.Value = CacheHelper.Instance.GetCounterValueHttp(performanceCounter);

            Assert.IsTrue(metric.Value >= 0);
        }
    }
}
