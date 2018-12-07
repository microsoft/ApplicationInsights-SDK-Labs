namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.Wcf.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Tests.Service;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClientTelemetryBindingElementTests
    {
        private const string TwoWayOp1 = "http://tempuri.org/ISimpleService/GetSimpleData";
        private const string TwoWayOp2 = "http://tempuri.org/ISimpleService/CallFailsWithFault";

        [TestMethod]
        [TestCategory("Client")]
        public void WhenClientIsNull_ConstructorThrowsException()
        {
            TelemetryClient client = null;
            ClientContract map = new ClientContract(typeof(ISimpleService));
            bool failed = false;
            try
            {
                var element = new ClientTelemetryBindingElement(client, map);
            }
            catch (ArgumentNullException)
            {
                failed = true;
            }

            Assert.IsTrue(failed, "Constructor did not throw ArgumentNullException");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenOperationMapIsNull_ConstructorThrowsException()
        {
            TelemetryClient client = new TelemetryClient();
            Type contractType = typeof(ISimpleService);
            ClientContract map = null;
            bool failed = false;
            try
            {
                var element = new ClientTelemetryBindingElement(client, map);
            }
            catch (ArgumentNullException)
            {
                failed = true;
            }

            Assert.IsTrue(failed, "Constructor did not throw ArgumentNullException");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenContextIsNull_CanBuildChannelFactoryThrowsException()
        {
            TelemetryClient client = new TelemetryClient();
            ClientContract map = new ClientContract(typeof(ISimpleService));
            bool failed = false;
            try
            {
                var element = new ClientTelemetryBindingElement(client, map);
                element.CanBuildChannelFactory<IRequestChannel>(null);
            }
            catch (ArgumentNullException)
            {
                failed = true;
            }

            Assert.IsTrue(failed, "CanBuildChannelFactory did not throw ArgumentNullException");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenContextIsNull_BuildChannelFactoryThrowsException()
        {
            TelemetryClient client = new TelemetryClient();
            ClientContract map = new ClientContract(typeof(ISimpleService));
            bool failed = false;
            try
            {
                var element = new ClientTelemetryBindingElement(client, map);
                element.BuildChannelFactory<IRequestChannel>(null);
            }
            catch (ArgumentNullException)
            {
                failed = true;
            }

            Assert.IsTrue(failed, "BuildChannelFactory did not throw ArgumentNullException");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WithIRequestChannel_CanBuildChannelFactoryReturnsTrue()
        {
            TestChannelShape<IRequestChannel>(new BasicHttpBinding());
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WithIRequestSessionChannel_CanBuildChannelFactoryReturnsTrue()
        {
            TestChannelShape<IRequestSessionChannel>(new WSHttpBinding());
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WithIOutputChannel_CanBuildChannelFactoryReturnsTrue()
        {
            TestChannelShape<IOutputChannel>(new NetMsmqBinding(), false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WithIOutputSessionChannel_CanBuildChannelFactoryReturnsTrue()
        {
            TestChannelShape<IOutputSessionChannel>(new NetMsmqBinding(), false);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WithIDuplexSessionChannel_CanBuildChannelFactoryReturnsTrue()
        {
            TestChannelShape<IDuplexSessionChannel>(new NetTcpBinding() { TransferMode = TransferMode.Buffered });
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WithIInputChannel_CanBuildChannelFactoryReturnsFalse()
        {
            TelemetryClient client = new TelemetryClient();
            ClientContract map = new ClientContract(typeof(ISimpleService));
            var element = new ClientTelemetryBindingElement(client, map);

            var custom = new CustomBinding(new NetMsmqBinding());
            BindingContext context = new BindingContext(custom, new BindingParameterCollection());
            Assert.IsFalse(element.CanBuildChannelFactory<IInputChannel>(context));
        }

        public void TestChannelShape<TChannel>(Binding binding, bool tryCreate = true)
        {
            TelemetryClient client = new TelemetryClient();
            ClientContract map = new ClientContract(typeof(ISimpleService));
            var element = new ClientTelemetryBindingElement(client, map);

            var custom = new CustomBinding(binding);
            BindingContext context = new BindingContext(custom, new BindingParameterCollection());
            Assert.IsTrue(element.CanBuildChannelFactory<TChannel>(context));

            if (tryCreate)
            {
                var factory = element.BuildChannelFactory<TChannel>(context);
                Assert.IsNotNull(factory, "BuildChannelFactory() returned null");
                factory.Close();
            }
        }
    }
}
