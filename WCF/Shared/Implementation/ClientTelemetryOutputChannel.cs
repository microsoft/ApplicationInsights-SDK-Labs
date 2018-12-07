namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal sealed class ClientTelemetryOutputChannel : ClientTelemetryChannelBase, IOutputChannel, IOutputSessionChannel
    {
        public ClientTelemetryOutputChannel(IChannelManager channelManager, IChannel channel)
            : base(channelManager, channel)
        {
        }

        public override EndpointAddress RemoteAddress
        {
            get { return this.OutputChannel.RemoteAddress; }
        }

        public Uri Via
        {
            get { return this.OutputChannel.Via; }
        }

        public IOutputSession Session
        {
            get { return ((IOutputSessionChannel)this.InnerChannel).Session; }
        }

        private IOutputChannel OutputChannel
        {
            get { return (IOutputChannel)this.InnerChannel; }
        }

        public void Send(Message message)
        {
            this.Send(message, this.ChannelManager.SendTimeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.Send));
            var telemetry = StartSendTelemetry(message, nameof(this.Send));
            try
            {
                this.OutputChannel.Send(message, timeout);
                this.StopSendTelemetry(telemetry, null, null, nameof(this.Send));
            }
            catch (Exception ex)
            {
                this.StopSendTelemetry(telemetry, null, ex, nameof(this.Send));
                throw;
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, this.ChannelManager.SendTimeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.BeginSend));
            var telemetry = StartSendTelemetry(message, nameof(this.BeginSend));
            try
            {
                return new SendAsyncResult(this.OutputChannel, message, timeout, this.OnSendComplete, callback, state, telemetry);
            }
            catch (Exception ex)
            {
                this.StopSendTelemetry(telemetry, null, ex, nameof(this.BeginSend));
                throw;
            }
        }

        public void EndSend(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.EndSend));
            SendAsyncResult.End<SendAsyncResult>(result);
        }

        private void OnSendComplete(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var sar = (SendAsyncResult)result;
            this.StopSendTelemetry(sar.Telemetry, null, sar.LastException, nameof(this.OnSendComplete));
        }
    }
}
