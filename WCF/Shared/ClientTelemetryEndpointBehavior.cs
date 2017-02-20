using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Instruments client-side WCF endpoints to generate DependencyTelemetry
    /// events on calls.
    /// </summary>
    public sealed class ClientTelemetryEndpointBehavior : IEndpointBehavior
    {
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Gets or sets the name of the HTTP header to get root operation Id from.
        /// </summary>
        public String RootOperationIdHeaderName { get; set; }
        /// <summary>
        /// Gets or sets the name of the HTTP header to get parent operation Id from.
        /// </summary>
        public String ParentOperationIdHeaderName { get; set; }
        /// <summary>
        /// Gets or sets the name of the SOAP header to get root operation Id from.
        /// </summary>
        public String SoapRootOperationIdHeaderName { get; set; }
        /// <summary>
        /// Gets or sets the name of the SOAP header to get parent operation Id from.
        /// </summary>
        public String SoapParentOperationIdHeaderName { get; set; }
        /// <summary>
        /// Gets or sets the name of the SOAP header to get parent operation Id from.
        /// </summary>
        public String SoapHeaderNamespace { get; set; }

        /// <summary>
        /// Initializes a new instance using the default
        /// Application Insights configuration
        /// </summary>
        public ClientTelemetryEndpointBehavior()
            : this(TelemetryConfiguration.Active)
        {

        }

        /// <summary>
        /// Initializes a new instance using the specified configuration
        /// </summary>
        /// <param name="configuration">The Application Insights configuration to use</param>
        public ClientTelemetryEndpointBehavior(TelemetryConfiguration configuration)
            : this(configuration != null ? new TelemetryClient(configuration) : null)
        {
        }

        /// <summary>
        /// Initializes a new instance using the specified telemetry client
        /// </summary>
        /// <param name="client">The TelemetryClient instance to use to emit telemetry events</param>
        public ClientTelemetryEndpointBehavior(TelemetryClient client)
        {
            if ( client == null )
            {
                throw new ArgumentNullException(nameof(client));
            }
            this.telemetryClient = client;
            this.telemetryClient.Context.GetInternalContext().SdkVersion = "wcf: " + SdkVersionUtils.GetAssemblyVersion();
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            var contract = endpoint.Contract.ContractType;

            WcfEventSource.Log.ClientTelemetryApplied(contract.FullName);

            var description = new ClientContract(endpoint.Contract);
            var element = new ClientTelemetryBindingElement(telemetryClient, description);
            element.RootOperationIdHeaderName = RootOperationIdHeaderName;
            element.ParentOperationIdHeaderName = ParentOperationIdHeaderName;
            element.SoapRootOperationIdHeaderName = SoapRootOperationIdHeaderName;
            element.SoapParentOperationIdHeaderName = SoapParentOperationIdHeaderName;
            element.SoapHeaderNamespace = SoapHeaderNamespace;

            var collection = endpoint.Binding.CreateBindingElements();
            collection.Insert(0, element);
            endpoint.Binding = new CustomBinding(collection);
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
    }

}
