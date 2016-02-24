using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    [TestClass]
    public class FullStackTests
    {
        [TestMethod]
        [TestCategory("Integration"), TestCategory("Sync")]
        public void TelemetryEventsAreGeneratedOnServiceCall()
        {
            TestTelemetryChannel.Clear();
            using ( var host = new HostingContext<SimpleService, ISimpleService>() )
            {
                host.Open();
                ISimpleService client = host.GetChannel();
                client.GetSimpleData();
                Assert.IsTrue(TestTelemetryChannel.CollectedData().Count > 0);
            }
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Sync")]
        public void OperationNameIsSetBasedOnOperationCalled()
        {
            TestTelemetryChannel.Clear();
            using ( var host = new HostingContext<SimpleService, ISimpleService>() )
            {
                host.Open();
                ISimpleService client = host.GetChannel();
                client.GetSimpleData();
            }
            var operationName = TestTelemetryChannel.CollectedData()
                               .Select(x => x.Context.Operation.Name)
                               .First();

            Assert.IsTrue(operationName.EndsWith("GetSimpleData"));
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Sync")]
        public void AllTelemetryEventsFromOneCallHaveSameOperationId()
        {
            TestTelemetryChannel.Clear();
            using ( var host = new HostingContext<SimpleService, ISimpleService>() )
            {
                host.Open();
                ISimpleService client = host.GetChannel();
                client.GetSimpleData();
            }
            var ids = TestTelemetryChannel.CollectedData()
                    .Select(x => x.Context.Operation.Id)
                    .Distinct();

            Assert.AreEqual(1, ids.Count());
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Sync")]
        public void ErrorTelemetryEventsAreGeneratedOnFault()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<SimpleService, ISimpleService>()
                      .ShouldWaitForCompletion();
            using ( host )
            {
                host.Open();
                ISimpleService client = host.GetChannel();
                try
                {
                    client.CallFailsWithFault();
                } catch
                {
                }
            }
            var errors = from item in TestTelemetryChannel.CollectedData()
                         where item is ExceptionTelemetry
                         select item;
            Assert.IsTrue(errors.Count() > 0);
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Sync")]
        public void ErrorTelemetryEventsContainDetailedInfo()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<SimpleService, ISimpleService>()
                      .ShouldWaitForCompletion();
            using ( host )
            {
                host.Open();
                ISimpleService client = host.GetChannel();
                try
                {
                    client.CallFailsWithFault();
                } catch
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
        [TestCategory("Integration"), TestCategory("Sync")]
        public void ErrorTelemetryEventsContainDetailedInfoOnTypedFault()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<SimpleService, ISimpleService>()
                      .ShouldWaitForCompletion();
            using ( host )
            {
                host.Open();
                ISimpleService client = host.GetChannel();
                try
                {
                    client.CallFailsWithTypedFault();
                } catch
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
        [TestCategory("Integration"), TestCategory("Sync")]
        public void ErrorTelemetryEventsAreGeneratedOnExceptionAndIEDIF_False()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<SimpleService, ISimpleService>()
                      .ShouldWaitForCompletion();
            using ( host )
            {
                host.Open();
                ISimpleService client = host.GetChannel();
                try
                {
                    client.CallFailsWithException();
                } catch
                {
                }
            }
            var errors = from item in TestTelemetryChannel.CollectedData()
                         where item is ExceptionTelemetry
                         select item;
            Assert.IsTrue(errors.Count() > 0);
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Sync")]
        public void ErrorTelemetryEventsAreGeneratedOnExceptionAndIEDIF_True()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<SimpleService, ISimpleService>()
                      .ShouldWaitForCompletion()
                      .IncludeDetailsInFaults();
            using ( host )
            {
                host.Open();

                ISimpleService client = host.GetChannel();
                try
                {
                    client.CallFailsWithException();
                } catch
                {
                }
            }
            var errors = from item in TestTelemetryChannel.CollectedData()
                         where item is ExceptionTelemetry
                         select item;
            Assert.IsTrue(errors.Count() > 0);
        }



        [TestMethod]
        [TestCategory("Integration"), TestCategory("OperationTelemetry")]
        public void CallsToOpMarkedWithOperationTelemetryGeneratesEvents()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<SelectiveTelemetryService, ISelectiveTelemetryService>();
            using ( host )
            {
                host.Open();

                ISelectiveTelemetryService client = host.GetChannel();
                client.OperationWithTelemetry();
            }
            Assert.IsTrue(TestTelemetryChannel.CollectedData().Count > 0);
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("OperationTelemetry")]
        public void CallsToOpWithoutOperationTelemetryGeneratesEvents()
        {
            TestTelemetryChannel.Clear();
            var host = new HostingContext<SelectiveTelemetryService, ISelectiveTelemetryService>();
            using ( host )
            {
                host.Open();

                ISelectiveTelemetryService client = host.GetChannel();
                client.OperationWithoutTelemetry();
            }
            Assert.AreEqual(0, TestTelemetryChannel.CollectedData().Count);
        }


    }
}
