namespace AggregateMetrics.Tests.AzureWebApp
{
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class SumUpGaugeTests
    {
        [TestMethod]
        public void SumUpGaugeGetValueAndResetWorking()
        {
            SumUpGauge twoTimesPrivateBytes = new SumUpGauge("twoTimesPrivateBytes", new FlexiblePerformanceCounterGauge("privateBytes", new CacheHelperTests()), new FlexiblePerformanceCounterGauge("privateBytes", new CacheHelperTests()));
            FlexiblePerformanceCounterGauge privateBytes = new FlexiblePerformanceCounterGauge("privateBytes", new CacheHelperTests());

            MetricTelemetry expectedTelemetry = privateBytes.GetValueAndReset();
            MetricTelemetry actualTelemetry = twoTimesPrivateBytes.GetValueAndReset();

            // twoTimesPrivateBytes is -greater than (privateBytes * 1.85) but lower than (privateBytes * 2.15).
            Assert.IsTrue((expectedTelemetry.Value * 1.85) < actualTelemetry.Value && (expectedTelemetry.Value * 2.15) > actualTelemetry.Value);
        }
    }
}
