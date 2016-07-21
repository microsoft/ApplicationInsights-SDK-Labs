namespace webAppTest
{
    using System;
    using System.Diagnostics;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class UnitTestAzureWeb
    {
        [TestMethod]
        public void TestGetValueAndReset()
        {
            FlexiblePerformanceCounterGauge testingGage = new FlexiblePerformanceCounterGauge();

            var metric= testingGage.GetValueAndReset("privateBytes");
        
            Debug.WriteLine("mt contents:");
            Debug.WriteLine(metric);

            Assert.IsTrue(testingGage.GetValueAndReset("privateBytes") is Double );
        }
    }
}
