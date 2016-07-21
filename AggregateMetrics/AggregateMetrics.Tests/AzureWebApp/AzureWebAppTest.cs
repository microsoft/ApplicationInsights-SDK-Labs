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
            FlexiblePerformanceCounterGauge testingGage = new FlexiblePerformanceCounterGauge("privateBytes");

            var metric= testingGage.GetValueAndReset();
        
            Debug.WriteLine("mt contents:");
            Debug.WriteLine(metric);
        }
    }
}
