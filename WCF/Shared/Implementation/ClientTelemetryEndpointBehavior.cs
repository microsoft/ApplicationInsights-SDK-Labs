using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class ClientTelemetryEndpointBehavior : IEndpointBehavior
    {
        private TelemetryClient telemetryClient;

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
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            if ( endpoint == null )
            {
                throw new ArgumentNullException(nameof(endpoint));
            }
            if ( clientRuntime == null )
            {
                throw new ArgumentNullException(nameof(clientRuntime));
            }

            WcfEventSource.Log.ClientTelemetryApplied(endpoint.Contract.ContractType.FullName);

            var description = BuildDescription(endpoint);
            clientRuntime.MessageInspectors.Add(new ClientCallMessageInspector(telemetryClient, description));
            clientRuntime.ChannelInitializers.Add(new ClientChannelOpenTracker(telemetryClient, endpoint.Contract.ContractType));
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
