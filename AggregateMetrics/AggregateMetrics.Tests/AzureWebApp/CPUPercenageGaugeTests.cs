namespace AggregateMetrics.Tests.AzureWebApp
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
    using System.Threading;

    [TestClass]
    public class CPUPercenageGaugeTests
    {
        [TestMethod]
        public void BasicValidation()
        {
            CPUPercenageGauge gauge = new CPUPercenageGauge(
                "CPU",
                new PerformanceCounterFromJsonGauge(@"\Process(??APP_WIN32_PROC??)\Private Bytes * 2", "userTime", AzureWebApEnvironmentVariables.App, new CacheHelperTests()));

            var value1 = gauge.GetValueAndReset();

            Assert.IsTrue(Math.Abs(value1.Value) < 0.000001);

            Thread.Sleep(TimeSpan.FromSeconds(10));

            var value2 = gauge.GetValueAndReset();
            Assert.IsTrue(Math.Abs(value2.Value - ((24843750 - 24062500.0) / TimeSpan.FromSeconds(10).Ticks * 100.0)) < 0.0001, 
                string.Format("Actual: {0}, Expected: {1}", value2.Value, (24843750 - 24062500.0) / TimeSpan.FromSeconds(10).Ticks));
        }
    }
}
