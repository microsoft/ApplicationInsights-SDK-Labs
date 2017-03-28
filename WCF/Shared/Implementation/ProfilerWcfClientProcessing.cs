namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    internal sealed class ProfilerWcfClientProcessing
    {
        private WcfDependencyTrackingTelemetryModule trackingModule;

        public ProfilerWcfClientProcessing(WcfDependencyTrackingTelemetryModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            this.trackingModule = module;
        }

        public object OnStartInitializeEndpoint1(object thisObj, object serviceEndpoint)
        {
            return null;
        }

        public object OnEndInitializeEndpoint1(object context, object returnValue, object thisObj, object serviceEndpoint)
        {
            if (thisObj == null)
            {
                WcfClientEventSource.Log.NotExpectedCallback(0, nameof(this.OnEndInitializeEndpoint1), "thisObj == null");
                return returnValue;
            }

            this.AddBehaviorIfNotPresent(((ChannelFactory)thisObj).Endpoint);
            return returnValue;
        }

        public object OnStartInitializeEndpoint2(object thisObj, object configuratioNameOrBinding, object address)
        {
            Console.WriteLine("onStartInitializeEndpoint2");
            return null;
        }

        public object OnEndInitializeEndpoint2(object context, object returnValue, object thisObj, object configuratioNameOrBinding, object address)
        {
            if (thisObj == null)
            {
                WcfClientEventSource.Log.NotExpectedCallback(0, nameof(this.OnEndInitializeEndpoint2), "thisObj == null");
                return returnValue;
            }

            this.AddBehaviorIfNotPresent(((ChannelFactory)thisObj).Endpoint);
            return returnValue;
        }

        public object OnStartInitializeEndpoint3(object thisObj, object configurationName, object address, object configuration)
        {
            return null;
        }

        public object OnEndInitializeEndpoint3(object context, object returnValue, object thisObj, object configurationName, object address, object configuration)
        {
            if (thisObj == null)
            {
                WcfClientEventSource.Log.NotExpectedCallback(0, nameof(this.OnEndInitializeEndpoint3), "thisObj == null");
                return returnValue;
            }

            this.AddBehaviorIfNotPresent(((ChannelFactory)thisObj).Endpoint);
            return returnValue;
        }

        private void AddBehaviorIfNotPresent(ServiceEndpoint endpoint)
        {
            if (endpoint.Behaviors.OfType<ClientTelemetryEndpointBehavior>().Any())
            {
                // don't add behavior if it's already been added by user code
                // or the configuration
                return;
            }

            var behavior = new ClientTelemetryEndpointBehavior(this.trackingModule.TelemetryClient)
            {
                RootOperationIdHeaderName = this.trackingModule.RootOperationIdHeaderName,
                ParentOperationIdHeaderName = this.trackingModule.ParentOperationIdHeaderName,
                SoapRootOperationIdHeaderName = this.trackingModule.SoapRootOperationIdHeaderName,
                SoapParentOperationIdHeaderName = this.trackingModule.SoapParentOperationIdHeaderName,
                SoapHeaderNamespace = this.trackingModule.SoapHeaderNamespace
            };
            endpoint.Behaviors.Add(behavior);
        }
    }
}
