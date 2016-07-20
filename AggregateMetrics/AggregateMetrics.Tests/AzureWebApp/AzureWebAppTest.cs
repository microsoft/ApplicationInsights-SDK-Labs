namespace webAppTest
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Diagnostics;
    [TestClass]
    public class UnitTestAzureWeb
    {
        [TestMethod]
        public void TestGetValueAndReset()
        {
            PerformanceCounterGauge testingGage = new PerformanceCounterGauge();

            var mt= testingGage.GetValueAndReset();
        
            Debug.WriteLine("mt contents:");
            Debug.WriteLine(mt.Value);
            Debug.WriteLine(mt.Name);

            Assert.IsTrue(testingGage.GetValueAndReset() is MetricTelemetry );
        }
    }
}
