namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Implementation;

    /// <summary>
    /// Instruments client-side WCF endpoints to generate DependencyTelemetry
    /// events on calls.
    /// </summary>
    public sealed class ClientTelemetryEndpointBehavior : IEndpointBehavior
    {
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTelemetryEndpointBehavior" />
        /// class using the default Application Insights configuration.
        /// </summary>
        public ClientTelemetryEndpointBehavior()
            : this(TelemetryConfiguration.Active)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTelemetryEndpointBehavior" />
        /// class using the specified configuration.
        /// </summary>
        /// <param name="configuration">The Application Insights configuration to use.</param>
        public ClientTelemetryEndpointBehavior(TelemetryConfiguration configuration)
            : this(configuration != null ? new TelemetryClient(configuration) : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientTelemetryEndpointBehavior"/> class
        /// using the specified telemetry client.
        /// </summary>
        /// <param name="client">The TelemetryClient instance to use to emit telemetry events.</param>
        public ClientTelemetryEndpointBehavior(TelemetryClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client)); 
            }

            this.telemetryClient = client;
            this.telemetryClient.Context.GetInternalContext().SdkVersion = "wcf: " + SdkVersionUtils.GetAssemblyVersion();
        }

        /// <summary>
        /// Gets or sets the name of the HTTP header to get root operation Id from.
        /// </summary>
        public string RootOperationIdHeaderName { get; set; }

        /// <summary>
        /// Gets or sets the name of the HTTP header to get parent operation Id from.
        /// </summary>
        public string ParentOperationIdHeaderName { get; set; }

        /// <summary>
        /// Gets or sets the name of the SOAP header to get root operation Id from.
        /// </summary>
        public string SoapRootOperationIdHeaderName { get; set; }

        /// <summary>
        /// Gets or sets the name of the SOAP header to get parent operation Id from.
        /// </summary>
        public string SoapParentOperationIdHeaderName { get; set; }

        /// <summary>
        /// Gets or sets the name of the SOAP header to get parent operation Id from.
        /// </summary>
        public string SoapHeaderNamespace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether channel events (such as channel open) should be emitted as dependencies.
        /// </summary>
        public bool IgnoreChannelEvents { get; set; }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            var contract = endpoint.Contract.ContractType;

            WcfClientEventSource.Log.ClientTelemetryApplied(contract.FullName);

            // in most cases, we'll create the description only once
            // since channel factories are cached by default in ClientBase<T>.
            // We could possibly cache this to avoid the hit in other scenarios.
            var description = new ClientContract(endpoint.Contract);
            var element = new ClientTelemetryBindingElement(this.telemetryClient, description)
            {
                RootOperationIdHeaderName = this.RootOperationIdHeaderName,
                ParentOperationIdHeaderName = this.ParentOperationIdHeaderName,
                SoapRootOperationIdHeaderName = this.SoapRootOperationIdHeaderName,
                SoapParentOperationIdHeaderName = this.SoapParentOperationIdHeaderName,
                SoapHeaderNamespace = this.SoapHeaderNamespace,
            };
            var collection = endpoint.Binding.CreateBindingElements();
            collection.Insert(0, element);
            endpoint.Binding = new CustomBinding(collection);
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            throw new NotSupportedException("Client telemetry cannot be used on the server-side stack.");
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }
    }
}
