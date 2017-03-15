namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    using System;
    using System.Linq;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
    using Microsoft.ApplicationInsights.Wcf.Tests.Service;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

#if NET45
    [TestClass]
    public class AsyncStackTests
    {
        [TestMethod]
        [TestCategory("Integration"), TestCategory("Async")]
        public void TelemetryEventsAreGeneratedOnAsyncCall()
        {
            TestTelemetryChannel.Clear();
            using (var host = new HostingContext<AsyncService, IAsyncService>())
            {
                host.Open();
                IAsyncService client = host.GetChannel();
                client.GetDataAsync().Wait();
                Assert.IsTrue(TestTelemetryChannel.CollectedData().Count > 0);
            }
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Async")]
        public void ErrorTelemetryEventsAreGeneratedOnAsyncFault()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<AsyncService, IAsyncService>()
                      .ShouldWaitForCompletion();
            using (host)
            {
                host.Open();
                IAsyncService client = host.GetChannel();
                try
                {
                    client.FailWithFaultAsync().Wait();
                }
                catch
                {
                }
            }

            var errors = from item in TestTelemetryChannel.CollectedData()
                         where item is ExceptionTelemetry
                         select item;
            Assert.IsTrue(errors.Count() > 0);
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Async")]
        public void ErrorTelemetryEventsContainAllDataOnAsyncCall()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<AsyncService, IAsyncService>()
                      .ShouldWaitForCompletion();
            using (host)
            {
                host.Open();
                IAsyncService client = host.GetChannel();
                try
                {
                    client.FailWithFaultAsync().Wait();
                }
                catch
                {
                }
            }

            var error = (from item in TestTelemetryChannel.CollectedData()
                         where item is ExceptionTelemetry
                         select item).Cast<ExceptionTelemetry>().First();
            Assert.IsNotNull(error.Exception);
            Assert.IsNotNull(error.Context.Operation.Id);
            Assert.IsNotNull(error.Context.Operation.Name);
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Async")]
        public void ErrorTelemetryEventsAreGeneratedOnAsyncExceptionAndIEDIF_False()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<AsyncService, IAsyncService>()
                      .ShouldWaitForCompletion();
            using (host)
            {
                host.Open();
                IAsyncService client = host.GetChannel();
                try
                {
                    client.FailWithExceptionAsync().Wait();
                }
                catch
                {
                }
            }

            var errors = from item in TestTelemetryChannel.CollectedData()
                         where item is ExceptionTelemetry
                         select item;
            Assert.IsTrue(errors.Count() > 0);
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Async")]
        public void ErrorTelemetryEventsAreGeneratedOnAsyncExceptionAndIEDIF_True()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<AsyncService, IAsyncService>()
                      .ShouldWaitForCompletion()
                      .IncludeDetailsInFaults();
            using (host)
            {
                host.Open();
                IAsyncService client = host.GetChannel();
                try
                {
                    client.FailWithExceptionAsync().Wait();
                }
                catch
                {
                }
            }

            var errors = from item in TestTelemetryChannel.CollectedData()
                         where item is ExceptionTelemetry
                         select item;
            Assert.IsTrue(errors.Count() > 0);
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Async")]
        public void TelemetryContextIsFlowedAccrossAsyncCalls()
        {
            TestTelemetryChannel.Clear();
            using (var host = new HostingContext<AsyncService, IAsyncService>())
            {
                host.Open();
                IAsyncService client = host.GetChannel();
                client.WriteDependencyEventAsync().Wait();
            }

            var data = TestTelemetryChannel.CollectedData();
            var request = data
                         .OfType<RequestTelemetry>()
                         .First();
            var dependency = data
                            .OfType<DependencyTelemetry>()
                            .FirstOrDefault();
            Assert.AreEqual(request.Context.Operation.Id, dependency.Context.Operation.Id);
            Assert.AreEqual(request.Context.Operation.Name, dependency.Context.Operation.Name);
        }
    }
#endif // NET45
}
