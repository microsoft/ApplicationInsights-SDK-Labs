using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    sealed class ClientTelemetryRequestChannel : ClientTelemetryChannelBase, IRequestChannel, IRequestSessionChannel
    {
        private IRequestChannel RequestChannel
        {
            get { return (IRequestChannel)InnerChannel; }
        }

        public override EndpointAddress RemoteAddress
        {
            get { return RequestChannel.RemoteAddress; }
        }
        public Uri Via
        {
            get { return RequestChannel.Via; }
        }

        public IOutputSession Session
        {
            get { return ((IRequestSessionChannel)InnerChannel).Session; }
        }

        public ClientTelemetryRequestChannel(TelemetryClient client, IChannel channel, Type contractType, ClientOperationMap map)
            : base(client, channel, contractType, map)
        {
        }

        public Message Request(Message message)
        {
            var telemetry = StartSendTelemetry(message, nameof(Request));
            try
            {
                var response = RequestChannel.Request(message);
                StopSendTelemetry(telemetry, response, null, nameof(message));
                return response;
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(message));
                throw;
            }
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            var telemetry = StartSendTelemetry(message, nameof(Request));
            try
            {
                var response = RequestChannel.Request(message, timeout);
                StopSendTelemetry(telemetry, response, null, nameof(message));
                return response;
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(message));
                throw;
            }
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            var telemetry = StartSendTelemetry(message, nameof(BeginRequest));
            try
            {
                var result = RequestChannel.BeginRequest(message, callback, state);
                return new NestedAsyncResult(result, telemetry);
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(BeginRequest));
                throw;
            }
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            var telemetry = StartSendTelemetry(message, nameof(BeginRequest));
            try
            {
                var result = RequestChannel.BeginRequest(message, timeout, callback, state);
                return new NestedAsyncResult(result, telemetry);
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(BeginRequest));
                throw;
            }
        }

        public Message EndRequest(IAsyncResult result)
        {
            var nar = (NestedAsyncResult)result;
            try
            {
                var response = RequestChannel.EndRequest(nar.Inner);
                StopSendTelemetry(nar.Telemetry, response, null, nameof(EndRequest));
                return response;
            } catch ( Exception ex )
            {
                StopSendTelemetry(nar.Telemetry, null, ex, nameof(EndRequest));
                throw;
            }
        }


    }
}
