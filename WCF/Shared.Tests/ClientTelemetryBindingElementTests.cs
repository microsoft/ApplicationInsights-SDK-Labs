using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.ApplicationInsights.Wcf.Tests.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class ClientTelemetryBindingElementTests
    {
        const String TwoWayOp1 = "http://tempuri.org/ISimpleService/GetSimpleData";
        const String TwoWayOp2 = "http://tempuri.org/ISimpleService/CallFailsWithFault";

        [TestMethod]
        [TestCategory("Client")]
        public void WhenClientIsNull_ConstructorThrowsException()
        {
            TelemetryClient client = null;
            Type contractType = typeof(ISimpleService);
            ClientOperationMap map = BuildOperationMap();
            bool failed = false;
            try
            {
                var element = new ClientTelemetryBindingElement(client, contractType, map);
            } catch ( ArgumentNullException )
            {
                failed = true;
            }
            Assert.IsTrue(failed, "Constructor did not throw ArgumentNullException");
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenContractIsNull_ConstructorThrowsException()
        {
            TelemetryClient client = new TelemetryClient();
            Type contractType = null;
            ClientOperationMap map = BuildOperationMap();
            bool failed = false;
            try
            {
                var element = new ClientTelemetryBindingElement(client, contractType, map);
            } catch ( ArgumentNullException )
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
            ClientOperationMap map = null;
            bool failed = false;
            try
            {
                var element = new ClientTelemetryBindingElement(client, contractType, map);
            } catch ( ArgumentNullException )
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
            Type contractType = typeof(ISimpleService);
            ClientOperationMap map = BuildOperationMap();
            bool failed = false;
            try
            {
                var element = new ClientTelemetryBindingElement(client, contractType, map);
                element.CanBuildChannelFactory<IRequestChannel>(null);
            } catch ( ArgumentNullException )
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
            Type contractType = typeof(ISimpleService);
            ClientOperationMap map = BuildOperationMap();
            bool failed = false;
            try
            {
                var element = new ClientTelemetryBindingElement(client, contractType, map);
                element.BuildChannelFactory<IRequestChannel>(null);
            } catch ( ArgumentNullException )
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
            Type contractType = typeof(ISimpleService);
            ClientOperationMap map = BuildOperationMap();
            var element = new ClientTelemetryBindingElement(client, contractType, map);

            var custom = new CustomBinding(new NetMsmqBinding());
            BindingContext context = new BindingContext(custom, new BindingParameterCollection());
            Assert.IsFalse(element.CanBuildChannelFactory<IInputChannel>(context));
        }


        public void TestChannelShape<TChannel>(Binding binding, bool tryCreate = true)
        {
            TelemetryClient client = new TelemetryClient();
            Type contractType = typeof(ISimpleService);
            ClientOperationMap map = BuildOperationMap();
            var element = new ClientTelemetryBindingElement(client, contractType, map);

            var custom = new CustomBinding(binding);
            BindingContext context = new BindingContext(custom, new BindingParameterCollection());
            Assert.IsTrue(element.CanBuildChannelFactory<TChannel>(context));

            if ( tryCreate )
            {
                var factory = element.BuildChannelFactory<TChannel>(context);
                Assert.IsNotNull(factory, "BuildChannelFactory() returned null");
                factory.Close();
            }
        }

        private ClientOperationMap BuildOperationMap()
        {
            ClientOpDescription[] ops = new ClientOpDescription[]
            {
                new ClientOpDescription { Action = TwoWayOp1, IsOneWay = false, Name = "GetSimpleData" },
                new ClientOpDescription { Action = TwoWayOp2, IsOneWay = false, Name = "CallFailsWithFault" },
            };
            return new ClientOperationMap(ops);
        }
    }
}
