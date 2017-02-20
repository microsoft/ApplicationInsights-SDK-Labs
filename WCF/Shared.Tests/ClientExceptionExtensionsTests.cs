using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class ClientExceptionExtensionsTests
    {
        [TestMethod]
        public void When_ExceptionIsNull_ExceptionIsThrown()
        {
            bool failed = false;
            try
            {
                ClientExceptionExtensions.ToResultCode(null);
            } catch (ArgumentNullException ex)
            {
                failed = true;
            }
            Assert.IsTrue(failed, "ToResultCode() did not throw ArgumentNullException");
        }

        [TestMethod]
        public void When_TimeoutException()
        {
            TestException<TimeoutException>("Timeout");
        }

        [TestMethod]
        public void When_EndpointNotFoundException()
        {
            TestException<EndpointNotFoundException>("EndpointNotFound");
        }

        [TestMethod]
        public void When_ServerTooBusyException()
        {
            TestException<ServerTooBusyException>("ServerTooBusy");
        }

        [TestMethod]
        public void When_FaultException()
        {
            TestException<FaultException>("SoapFault");
        }

        [TestMethod]
        public void When_OtherException()
        {
            TestException<ChannelTerminatedException>("ChannelTerminatedException");
        }


        private void TestException<TException>(string expected) where TException : Exception, new()
        {
            var ex = new TException();
            Assert.AreEqual(expected, ex.ToResultCode());
        }
    }
}
