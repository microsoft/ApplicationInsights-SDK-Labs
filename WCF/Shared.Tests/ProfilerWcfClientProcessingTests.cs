namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Wcf.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Tests.Service;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProfilerWcfClientProcessingTests
    {
        [TestMethod]
        public void WhenInitializeEndpoint1IsCalled_BehaviorIsAdded()
        {
            ServiceEndpoint endpoint = CreateEndpoint();
            endpoint.Address = new EndpointAddress("http://localhost/Service1.svc");
            using (ChannelFactory factory = new ChannelFactory<ISimpleService>(endpoint))
            {
                var module = new WcfDependencyTrackingTelemetryModule();
                module.Initialize(TelemetryConfiguration.Active);

                var wcfProcessing = new ProfilerWcfClientProcessing(module);
                wcfProcessing.OnEndInitializeEndpoint1(null, null, factory, null);

                var behavior = endpoint.Behaviors.Find<ClientTelemetryEndpointBehavior>();
                Assert.IsNotNull(behavior, "Behavior was not added to endpoint");
            }
        }

        [TestMethod]
        public void WhenInitializeEndpoint1IsCalled_AndBehaviorExists_BehaviorIsNotAdded()
        {
            ServiceEndpoint endpoint = CreateEndpoint();
            endpoint.Address = new EndpointAddress("http://localhost/Service1.svc");
            endpoint.Behaviors.Add(new ClientTelemetryEndpointBehavior(TelemetryConfiguration.Active));

            using (ChannelFactory factory = new ChannelFactory<ISimpleService>(endpoint))
            {
                var module = new WcfDependencyTrackingTelemetryModule();
                module.Initialize(TelemetryConfiguration.Active);

                var wcfProcessing = new ProfilerWcfClientProcessing(module);
                wcfProcessing.OnEndInitializeEndpoint1(null, null, factory, null);

                var numBehaviors = endpoint.Behaviors.OfType<ClientTelemetryEndpointBehavior>().Count();
                Assert.AreEqual(1, numBehaviors, "Behavior was added to endpoint twice");
            }
        }

        [TestMethod]
        public void WhenInitializeEndpoint2IsCalled_BehaviorIsAdded()
        {
            ServiceEndpoint endpoint = CreateEndpoint();
            endpoint.Address = new EndpointAddress("http://localhost/Service1.svc");
            using (ChannelFactory factory = new ChannelFactory<ISimpleService>(endpoint))
            {
                var module = new WcfDependencyTrackingTelemetryModule();
                module.Initialize(TelemetryConfiguration.Active);

                var wcfProcessing = new ProfilerWcfClientProcessing(module);
                wcfProcessing.OnEndInitializeEndpoint2(null, null, factory, null, null);

                var behavior = endpoint.Behaviors.Find<ClientTelemetryEndpointBehavior>();
                Assert.IsNotNull(behavior, "Behavior was not added to endpoint");
            }
        }

        [TestMethod]
        public void WhenInitializeEndpoint3IsCalled_BehaviorIsAdded()
        {
            ServiceEndpoint endpoint = CreateEndpoint();
            endpoint.Address = new EndpointAddress("http://localhost/Service1.svc");
            using (ChannelFactory factory = new ChannelFactory<ISimpleService>(endpoint))
            {
                var module = new WcfDependencyTrackingTelemetryModule();
                module.Initialize(TelemetryConfiguration.Active);

                var wcfProcessing = new ProfilerWcfClientProcessing(module);
                wcfProcessing.OnEndInitializeEndpoint3(null, null, factory, null, null, null);

                var behavior = endpoint.Behaviors.Find<ClientTelemetryEndpointBehavior>();
                Assert.IsNotNull(behavior, "Behavior was not added to endpoint");
            }
        }

        private static ServiceEndpoint CreateEndpoint()
        {
            var contractDescription = ContractBuilder.CreateDescription(typeof(ISimpleService), typeof(SimpleService));
            return new ServiceEndpoint(contractDescription);
        }
    }
}
