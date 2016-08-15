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
        public void TestPerformanceCounterValuesAreCorrectlyRetrievedUsingPerformanceCounterFromJsonGauge()
        {
            PerformanceCounterFromJsonGauge gauge = new PerformanceCounterFromJsonGauge(@"\Process(??APP_WIN32_PROC??)\Private Bytes", "privateBytes", AzureWebApEnvironmentVariables.App,new CacheHelperTests());
            MetricTelemetry metric = gauge.GetValueAndReset();

            Assert.IsTrue(metric.Value > 0);
        }
    }
}
