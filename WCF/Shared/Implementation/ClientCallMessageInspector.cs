using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class ClientCallMessageInspector : IClientMessageInspector
    {
        private TelemetryClient telemetryClient;
        private ClientOperationMap operationMap;

        public ClientCallMessageInspector(TelemetryClient client, ClientOperationMap clientOperations)
        {
            telemetryClient = client;
            operationMap = clientOperations;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var soapAction = request.Headers.Action;
            ClientOpDescription operation;
            if ( !this.operationMap.TryLookupByAction(soapAction, out operation) )
            {
                return null;
            }

            try
            {
                var telemetry = new DependencyTelemetry();
                telemetry.Start();
                telemetry.Name = channel.RemoteAddress.Uri.ToString();
                telemetry.Target = channel.RemoteAddress.Uri.Host;
                telemetry.Data = operation.Name;
                telemetry.Type = DependencyConstants.WcfClientCall;
                telemetry.Properties["soapAction"] = soapAction;

                // For One-Way operations, AfterReceiveReply will
                // never get called, so we need to write the event now
                // This means that duration will be zero.
                if ( operation.IsOneWay )
                {
                    telemetry.Properties["isOneWay"] = "True";
                    telemetry.Stop();
                    telemetryClient.TrackDependency(telemetry);
                }

                return telemetry;
            } catch ( Exception ex )
            {
                WcfEventSource.Log.ClientInspectorError(nameof(BeforeSendRequest), ex.ToString());
                throw;
            }
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var telemetry = (DependencyTelemetry)correlationState;
            if ( telemetry != null )
            {
                try
                {
                    telemetry.Success = !reply.IsFault;
                    telemetry.Stop();
                    telemetryClient.TrackDependency(telemetry);
                } catch ( Exception ex )
                {
                    WcfEventSource.Log.ClientInspectorError(nameof(AfterReceiveReply), ex.ToString());
                    throw;
                }
            }
        }
    }
}
