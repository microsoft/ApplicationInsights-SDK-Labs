namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.Reflection;
    using System.Runtime.Remoting;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Wcf.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
    using Microsoft.ApplicationInsights.Wcf.Tests.Integration;
    using Microsoft.ApplicationInsights.Wcf.Tests.Service;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClientTelemetryEndpointBehaviorTests
    {
        [TestMethod]
        [TestCategory("Client")]
        public void BehaviorCreatesCustomBindingWithTimeouts()
        {
            var binding = new NetTcpBinding()
            {
                OpenTimeout = new TimeSpan(1, 0, 0),
                SendTimeout = new TimeSpan(2, 0, 0),
                ReceiveTimeout = new TimeSpan(3, 0, 0),
                CloseTimeout = new TimeSpan(4, 0, 0)
            };
            var contract = ContractBuilder.CreateDescription(typeof(ISimpleService), typeof(SimpleService));
            var ep = new ServiceEndpoint(contract, binding, new EndpointAddress("net.tcp://localhost:8765"));

            IEndpointBehavior behavior = new ClientTelemetryEndpointBehavior();
            behavior.AddBindingParameters(ep, new BindingParameterCollection());

            Assert.AreEqual(binding.OpenTimeout, ep.Binding.OpenTimeout);
            Assert.AreEqual(binding.SendTimeout, ep.Binding.SendTimeout);
            Assert.AreEqual(binding.ReceiveTimeout, ep.Binding.ReceiveTimeout);
            Assert.AreEqual(binding.CloseTimeout, ep.Binding.CloseTimeout);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void BehaviorAddsCustomBinding()
        {
            using (var host = new HostingContext<SimpleService, ISimpleService>())
            {
                var binding = new NetTcpBinding();
                var configuration = new TelemetryConfiguration();
                var factory = new ChannelFactory<ISimpleService>(binding, host.GetServiceAddress());
                ISimpleService channel = null;
                try
                {
                    var behavior = new ClientTelemetryEndpointBehavior(configuration);
                    factory.Endpoint.EndpointBehaviors.Add(behavior);

                    channel = factory.CreateChannel();
                    var innerChannel = GetInnerChannel(channel);
                    ((IClientChannel)channel).Close();
                    factory.Close();

                    Assert.IsInstanceOfType(innerChannel, typeof(ClientTelemetryChannelBase), "Telemetry channel is missing");
                }
                catch
                {
                    factory.Abort();
                    if (channel != null)
                    {
                        ((IClientChannel)channel).Abort();
                    }

                    throw;
                }
            }
        }

        [TestMethod]
        [TestCategory("Integration"), TestCategory("Client")]
        public void RequestReply_TelemetryIsWritten()
        {
            TestTelemetryChannel.Clear();
            using (var host = new HostingContext<SimpleService, ISimpleService>())
            {
                host.Open();

                var binding = new NetTcpBinding();
                var configuration = new TelemetryConfiguration();
                var factory = new ChannelFactory<ISimpleService>(binding, host.GetServiceAddress());
                ISimpleService channel = null;
                try
                {
                    var behavior = new ClientTelemetryEndpointBehavior(configuration);
                    factory.Endpoint.EndpointBehaviors.Add(behavior);

                    channel = factory.CreateChannel();
                    channel.GetSimpleData();
                    ((IClientChannel)channel).Close();
                    factory.Close();

                    Assert.IsTrue(TestTelemetryChannel.CollectedData().Count > 0, "No telemetry events written");
                }
                catch
                {
                    if (channel != null)
                    {
                        ((IClientChannel)channel).Abort();
                    }

                    factory.Abort();
                    throw;
                }
            }
        }

        private static System.ServiceModel.Channels.IChannel GetInnerChannel(object proxy)
        {
            // TransparentProxy -> ServiceChannelProxy -> ServiceChannel
            var realProxy = RemotingServices.GetRealProxy(proxy);
            var proxyType = realProxy.GetType();
            var field = proxyType.GetField("serviceChannel", BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            object serviceChannel = field.GetValue(realProxy);

            var prop = serviceChannel.GetType().GetProperty("InnerChannel", BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Instance);
            return prop.GetValue(serviceChannel, null) as System.ServiceModel.Channels.IChannel;
        }
    }
}
