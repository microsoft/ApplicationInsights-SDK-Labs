using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
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
            telemetryClient = new TelemetryClient(configuration);
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            var description = BuildDescription(endpoint);
            clientRuntime.MessageInspectors.Add(new ClientCallMessageInspector(telemetryClient, description));

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
