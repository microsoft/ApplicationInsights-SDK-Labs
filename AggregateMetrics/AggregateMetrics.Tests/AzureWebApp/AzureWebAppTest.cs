namespace webAppTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Diagnostics;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
    [TestClass]
    public class UnitTestAzureWeb
    {
        [TestMethod]
        public void TestGetValueAndReset()
        {
            FlexiblePerformanceCounterGauge testingGage = new FlexiblePerformanceCounterGauge();

            var metric= testingGage.GetRawCounterValue("privateBytes");
        
            Debug.WriteLine("mt contents:");
            Debug.WriteLine(metric.Value);
            Debug.WriteLine(metric.Name);

            Assert.IsTrue(testingGage.GetRawCounterValue("privateBytes") is MetricTelemetry );
        }
    }
}
