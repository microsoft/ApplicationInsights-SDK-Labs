using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class ClientCallMessageInspector : IClientMessageInspector
    {
        private TelemetryClient telemetryClient;

        public ClientCallMessageInspector(TelemetryClient client)
        {
            telemetryClient = client;
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            return null;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }
    }
}
