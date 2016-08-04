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

            FlexiblePerformanceCounterGauge gauge = new FlexiblePerformanceCounterGauge(performanceCounter, new CacheHelperTests());
            MetricTelemetry metric = gauge.GetValueAndReset();

            Assert.IsTrue(metric.Value > 0);
        }
    }
}
