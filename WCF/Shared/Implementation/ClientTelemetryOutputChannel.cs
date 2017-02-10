using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    sealed class ClientTelemetryOutputChannel : ClientTelemetryChannelBase, IOutputChannel, IOutputSessionChannel
    {
        private IOutputChannel OutputChannel
        {
            get { return (IOutputChannel)InnerChannel; }
        }

        public override EndpointAddress RemoteAddress
        {
            get { return OutputChannel.RemoteAddress; }
        }
        public Uri Via
        {
            get { return OutputChannel.Via; }
        }

        public IOutputSession Session
        {
            get { return ((IOutputSessionChannel)InnerChannel).Session; }
        }

        public ClientTelemetryOutputChannel(IChannelManager channelManager, IChannel channel)
            : base(channelManager, channel)
        {
        }

        public void Send(Message message)
        {
            var telemetry = StartSendTelemetry(message, nameof(Send));
            try
            {
                OutputChannel.Send(message);
                StopSendTelemetry(telemetry, null, null, nameof(Send));
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(Send));
                throw;
            }
        }

        public void Send(Message message, TimeSpan timeout)
        {
            var telemetry = StartSendTelemetry(message, nameof(Send));
            try
            {
                OutputChannel.Send(message, timeout);
                StopSendTelemetry(telemetry, null, null, nameof(Send));
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(Send));
                throw;
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            var telemetry = StartSendTelemetry(message, nameof(BeginSend));
            try
            {
                var result = OutputChannel.BeginSend(message, callback, state);
                return new NestedAsyncResult(result, telemetry);
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(BeginSend));
                throw;
            }
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            var telemetry = StartSendTelemetry(message, nameof(BeginSend));
            try
            {
                var result = OutputChannel.BeginSend(message, timeout, callback, state);
                return new NestedAsyncResult(result, telemetry);
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(BeginSend));
                throw;
            }
        }

        public void EndSend(IAsyncResult result)
        {
            var nar = (NestedAsyncResult)result;
            try
            {
                OutputChannel.EndSend(nar.Inner);
                StopSendTelemetry(nar.Telemetry, null, null, nameof(EndSend));
            } catch ( Exception ex )
            {
                StopSendTelemetry(nar.Telemetry, null, ex, nameof(EndSend));
                throw;
            }
        }
    }
}
