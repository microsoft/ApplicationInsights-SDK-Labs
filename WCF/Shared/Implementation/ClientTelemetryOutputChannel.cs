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
            Send(message, ChannelManager.SendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            if ( message == null )
            {
                throw new ArgumentNullException(nameof(message));
            }
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
            return BeginSend(message, ChannelManager.SendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if ( message == null )
            {
                throw new ArgumentNullException(nameof(message));
            }
            var telemetry = StartSendTelemetry(message, nameof(BeginSend));
            try
            {
                return new SendAsyncResult(OutputChannel, message, timeout, this.OnSendComplete, callback, state, telemetry);
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(BeginSend));
                throw;
            }
        }

        public void EndSend(IAsyncResult result)
        {
            if ( result == null )
            {
                throw new ArgumentNullException(nameof(result));
            }
            SendAsyncResult.End<SendAsyncResult>(result);
        }

        private void OnSendComplete(IAsyncResult result)
        {
            if ( result == null )
            {
                throw new ArgumentNullException(nameof(result));
            }
            var sar = (SendAsyncResult)result;
            StopSendTelemetry(sar.Telemetry, null, sar.LastException, nameof(OnSendComplete));
        }
    }
}
