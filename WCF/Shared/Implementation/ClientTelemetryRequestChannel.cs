namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    internal sealed class ClientTelemetryRequestChannel : ClientTelemetryChannelBase, IRequestChannel, IRequestSessionChannel
    {
        public ClientTelemetryRequestChannel(IChannelManager channelManager, IChannel channel)
            : base(channelManager, channel)
        {
        }

        public override EndpointAddress RemoteAddress
        {
            get { return this.RequestChannel.RemoteAddress; }
        }

        public Uri Via
        {
            get { return this.RequestChannel.Via; }
        }

        public IOutputSession Session
        {
            get { return ((IRequestSessionChannel)this.InnerChannel).Session; }
        }

        private IRequestChannel RequestChannel
        {
            get { return (IRequestChannel)this.InnerChannel; }
        }

        public Message Request(Message message)
        {
            return this.Request(message, this.ChannelManager.SendTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.Request));
            var telemetry = this.StartSendTelemetry(message, nameof(this.Request));
            try
            {
                var response = this.RequestChannel.Request(message, timeout);
                this.StopSendTelemetry(telemetry, response, null, nameof(this.Request));
                return response;
            }
            catch (Exception ex)
            {
                this.StopSendTelemetry(telemetry, null, ex, nameof(this.Request));
                throw;
            }
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, ChannelManager.SendTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.BeginRequest));
            var telemetry = this.StartSendTelemetry(message, nameof(this.BeginRequest));
            try
            {
                return new RequestAsyncResult(this.RequestChannel, message, timeout, this.OnRequestDone, callback, state, telemetry);
            }
            catch (Exception ex)
            {
                this.StopSendTelemetry(telemetry, null, ex, nameof(this.BeginRequest));
                throw;
            }
        }

        public Message EndRequest(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.EndRequest));
            return RequestAsyncResult.End<RequestAsyncResult>(result).Reply;
        }

        private void OnRequestDone(IAsyncResult result)
        {
            var rar = (RequestAsyncResult)result;
            this.StopSendTelemetry(rar.Telemetry, rar.Reply, rar.LastException, nameof(this.OnRequestDone));
        }
    }
}
