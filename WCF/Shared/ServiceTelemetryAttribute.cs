namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Wcf.Implementation;

    /// <summary>
    /// Enables Application Insights telemetry when applied on a
    /// WCF service class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ServiceTelemetryAttribute : Attribute, IServiceBehavior
    {
        /// <summary>
        /// Gets or sets the Application Insights instrumentation key.
        /// </summary>
        /// <remarks>
        /// You can use this as an alternative to setting the key in the ApplicationInsights.config file.
        /// </remarks>
        public string InstrumentationKey { get; set; }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHost)
        {
            try
            {
                TelemetryConfiguration configuration = TelemetryConfiguration.Active;
                if (!string.IsNullOrEmpty(this.InstrumentationKey))
                {
                    configuration.InstrumentationKey = this.InstrumentationKey;
                }

                var contractFilter = BuildFilter(serviceDescription);
                WcfInterceptor interceptor = null;
                foreach (ChannelDispatcher channelDisp in serviceHost.ChannelDispatchers)
                {
                    if (channelDisp.ErrorHandlers.OfType<WcfInterceptor>().Any())
                    {
                        // already added, ignore
                        continue;
                    }

                    if (interceptor == null)
                    {
                        interceptor = new WcfInterceptor(configuration, contractFilter);
                    }

                    channelDisp.ErrorHandlers.Insert(0, interceptor);
                    foreach (var ep in channelDisp.Endpoints)
                    {
                        if (!ep.IsSystemEndpoint)
                        {
                            ep.DispatchRuntime.MessageInspectors.Insert(0, interceptor);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WcfEventSource.Log.InitializationFailure(ex.ToString());
            }
        }

        void IServiceBehavior.Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        private static ContractFilter BuildFilter(ServiceDescription serviceDescription)
        {
            var contracts = from ep in serviceDescription.Endpoints
                            where !ep.IsSystemEndpoint
                            select ep.Contract;
            return new ContractFilter(contracts);
        }
    }
}
