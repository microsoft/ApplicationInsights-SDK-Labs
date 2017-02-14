using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class ClientTelemetryEndpointBehavior : IEndpointBehavior
    {
        private TelemetryClient telemetryClient;

        public String RootOperationIdHeaderName { get; set; }
        public String ParentOperationIdHeaderName { get; set; }
        public String SoapRootOperationIdHeaderName { get; set; }
        public String SoapParentOperationIdHeaderName { get; set; }
        public String SoapHeaderNamespace { get; set; }

        public ClientTelemetryEndpointBehavior(TelemetryConfiguration configuration)
            : this(configuration != null ? new TelemetryClient(configuration) : null)
        {
        }
        public ClientTelemetryEndpointBehavior(TelemetryClient client)
        {
            if ( client == null )
            {
                throw new ArgumentNullException(nameof(client));
            }
            this.telemetryClient = client;
            this.telemetryClient.Context.GetInternalContext().SdkVersion = "wcf: " + SdkVersionUtils.GetAssemblyVersion();
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            var contract = endpoint.Contract.ContractType;

            WcfEventSource.Log.ClientTelemetryApplied(contract.FullName);

            var description = BuildDescription(endpoint);
            var element = new ClientTelemetryBindingElement(telemetryClient, contract, description);
            element.RootOperationIdHeaderName = RootOperationIdHeaderName;
            element.ParentOperationIdHeaderName = ParentOperationIdHeaderName;
            element.SoapRootOperationIdHeaderName = SoapRootOperationIdHeaderName;
            element.SoapParentOperationIdHeaderName = SoapParentOperationIdHeaderName;
            element.SoapHeaderNamespace = SoapHeaderNamespace;

            var collection = endpoint.Binding.CreateBindingElements();
            collection.Insert(0, element);
            endpoint.Binding = new CustomBinding(collection);
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        public static ClientOperationMap BuildDescription(ServiceEndpoint endpoint)
        {
            ClientOpDescription[] op = new ClientOpDescription[endpoint.Contract.Operations.Count];
            for ( int i=0; i < op.Length; i++ )
            {
                op[i] = ClientOpDescription.FromDescription(endpoint.Contract.Operations[i]);
            }
            return new ClientOperationMap(op);
        }
    }

}
