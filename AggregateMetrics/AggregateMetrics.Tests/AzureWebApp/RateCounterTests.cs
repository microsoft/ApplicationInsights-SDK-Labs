namespace AggregateMetrics.Tests.AzureWebApp
{
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RateCounterTests
    {
        [TestMethod]
        public void RateCounterGaugeGetValueAndResetWorking()
        {
            RateCounterGauge privateBytesRate = new RateCounterGauge(@"\Process(??APP_WIN32_PROC??)\Private Bytes", "privateBytes", null, new CacheHelperTests());

            MetricTelemetry metric = privateBytesRate.GetValueAndReset();
            Assert.IsTrue(metric.Value == 0);

            System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(7));

            metric = privateBytesRate.GetValueAndReset();

            Assert.IsTrue(metric.Value != 0);
        }
    }
}
