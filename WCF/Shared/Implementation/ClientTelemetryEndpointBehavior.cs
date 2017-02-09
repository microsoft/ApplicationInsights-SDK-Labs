using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class ClientTelemetryEndpointBehavior : IEndpointBehavior
    {
        public TelemetryClient telemetryClient;

        public ClientTelemetryEndpointBehavior(TelemetryConfiguration configuration)
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            telemetryClient = new TelemetryClient(configuration);
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            var contract = endpoint.Contract.ContractType;

            WcfEventSource.Log.ClientTelemetryApplied(contract.FullName);

            var description = BuildDescription(endpoint);
            var element = new ClientTelemetryBindingElement(telemetryClient, contract, description);
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
