namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;
    using Microsoft.ApplicationInsights.DataContracts;

    // An IRequestChannel is simulated on top of a duplex channel is
    // done by DuplexChannelBinder.Request() by correlating messages based on the message id
    // Equivalent to:
    //      Send Request
    //      Call TryReceive() in a loop until a message comes in or timeout expires
    //      Check RelatesTo == MessageId (otherwise an exception is thrown)
    // So in the end, this is equivalent to noticing that the response must come in right after
    // We're not a full duplex channel supporting duplex contracts, so we
    // cheat by assuming this.
    internal sealed class ClientTelemetryDuplexChannel : ClientTelemetryChannelBase, IDuplexChannel, IDuplexSessionChannel
    {
        private MessageCorrelator correlator;

        public ClientTelemetryDuplexChannel(IChannelManager channelManager, IChannel channel)
            : base(channelManager, channel)
        {
            this.correlator = new MessageCorrelator(this.OnRequestTimeout);
        }

        public EndpointAddress LocalAddress
        {
            get { return this.DuplexChannel.LocalAddress; }
        }

        public IDuplexSession Session
        {
            get { return ((IDuplexSessionChannel)this.DuplexChannel).Session; }
        }

        public Uri Via
        {
            get { return this.DuplexChannel.Via; }
        }

        public override EndpointAddress RemoteAddress
        {
            get { return this.DuplexChannel.RemoteAddress; }
        }

        private IDuplexChannel DuplexChannel
        {
            get { return (IDuplexChannel)this.InnerChannel; }
        }

        // ------------------------
        // Send side
        // ------------------------
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
                var isOneWay = IsOneWay(telemetry);
                this.DuplexChannel.Send(message, timeout);
                if (isOneWay)
                {
                    // no matching receive
                    this.StopSendTelemetry(telemetry, null, null, nameof(this.Send));
                }
                else
                {
                    this.correlator.Add(message.Headers.MessageId, telemetry, timeout);
                }
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
                var result = new SendAsyncResult(this.DuplexChannel, message, timeout, this.OnSendDone, callback, state, telemetry);
                this.correlator.Add(message.Headers.MessageId, telemetry, timeout);
                return result;
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

        // -------------------------------------------
        // Receive Side
        // -------------------------------------------
        // Both Receive and Receive(timeout) should fail with a 
        // if we get a timeout, we have no way of knowing which
        // outstanding message it closed :(
        public Message Receive()
        {
            return this.Receive(this.ChannelManager.ReceiveTimeout);
        }

        public Message Receive(TimeSpan timeout)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.Receive));
            var response = this.DuplexChannel.Receive(timeout);
            if (response != null)
            {
                this.HandleReply(response);
            }

            return response;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.TryReceive));
            var success = this.DuplexChannel.TryReceive(timeout, out message);
            if (success && message != null)
            {
                this.HandleReply(message);
            }

            return success;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.WaitForMessage));
            return this.DuplexChannel.WaitForMessage(timeout);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return this.BeginReceive(this.ChannelManager.ReceiveTimeout, callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.BeginReceive));
            return new ReceiveAsyncResult(this.DuplexChannel, timeout, null, callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.EndReceive));
            var rar = ReceiveAsyncResult.End<ReceiveAsyncResult>(result);
            if (rar.Message != null)
            {
                this.HandleReply(rar.Message);
            }

            return rar.Message;
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.BeginTryReceive));
            return new TryReceiveAsyncResult(this.DuplexChannel, timeout, null, callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.EndTryReceive));
            var trar = TryReceiveAsyncResult.End<TryReceiveAsyncResult>(result);
            message = trar.Message;
            if (trar.Result && message != null)
            {
                this.HandleReply(message);
            }

            return trar.Result;
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.BeginWaitForMessage));
            return this.DuplexChannel.BeginWaitForMessage(timeout, callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.EndWaitForMessage));
            return this.DuplexChannel.EndWaitForMessage(result);
        }

        protected override void OnClosed()
        {
            this.correlator.Dispose();
            base.OnClosed();
        }
        
        private void OnSendDone(IAsyncResult result)
        {
            var sar = (SendAsyncResult)result;

            if (this.IsOneWay(sar.Telemetry) || sar.LastException != null)
            {
                // not expecting reply
                this.correlator.Remove(sar.RequestId);
                this.StopSendTelemetry(sar.Telemetry, null, sar.LastException, nameof(this.OnSendDone));
            }
        }

        private void HandleReply(Message reply)
        {
            DependencyTelemetry telemetry = null;
            if (this.correlator.TryLookup(reply.Headers.RelatesTo, out telemetry))
            {
                this.StopSendTelemetry(telemetry, reply, null, nameof(this.HandleReply));
            }

            // not our message, leave it be
        }

        private void OnRequestTimeout(UniqueId messageId, DependencyTelemetry telemetry)
        {
            this.StopSendTelemetry(telemetry, null, new TimeoutException(), nameof(this.OnRequestTimeout));
        }
    }
}
