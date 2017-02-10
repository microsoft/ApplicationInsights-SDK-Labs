using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{

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

        private IDuplexChannel DuplexChannel
        {
            get { return (IDuplexChannel)InnerChannel; }
        }
        public EndpointAddress LocalAddress
        {
            get { return DuplexChannel.LocalAddress; }
        }
        public IDuplexSession Session
        {
            get { return ((IDuplexSessionChannel)DuplexChannel).Session; }
        }
        public Uri Via
        {
            get { return DuplexChannel.Via; }
        }

        public override EndpointAddress RemoteAddress
        {
            get { return DuplexChannel.RemoteAddress; }
        }

        public ClientTelemetryDuplexChannel(TelemetryClient client, IChannel channel, Type contractType, ClientOperationMap map)
            : base(client, channel, contractType, map)
        {
            correlator = new MessageCorrelator();
        }

        //
        // Send side
        //
        public void Send(Message message)
        {
            DoSend(message, () => DuplexChannel.Send(message));
        }

        public void Send(Message message, TimeSpan timeout)
        {
            DoSend(message, () => DuplexChannel.Send(message, timeout));
        }

        private void DoSend(Message message, Action sendCall)
        {
            var telemetry = StartSendTelemetry(message, nameof(DoSend));
            try
            {
                bool isOneWay = IsOneWay(telemetry);
                sendCall();
                if ( isOneWay )
                {
                    // no matching receive
                    StopSendTelemetry(telemetry, null, null, nameof(DoSend));
                } else
                {
                    correlator.Add(message.Headers.MessageId, telemetry);
                }
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(DoSend));
                throw;
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            var telemetry = StartSendTelemetry(message, nameof(BeginSend));
            try
            {
                var result = DuplexChannel.BeginSend(message, callback, state);
                correlator.Add(message.Headers.MessageId, telemetry);
                return new NestedAsyncResult(result, telemetry, message.Headers.MessageId);
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
                var result = DuplexChannel.BeginSend(message, timeout, callback, state);
                correlator.Add(message.Headers.MessageId, telemetry);
                return new NestedAsyncResult(result, telemetry, message.Headers.MessageId);
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
                DuplexChannel.EndSend(nar.Inner);
                if ( IsOneWay(nar.Telemetry) )
                {
                    // not expecting reply
                    correlator.Remove((UniqueId)nar.OtherState);
                    StopSendTelemetry(nar.Telemetry, null, null, nameof(EndSend));
                }
            } catch ( Exception ex )
            {
                // send failed, don't expect reply
                correlator.Remove((UniqueId)nar.OtherState);
                StopSendTelemetry(nar.Telemetry, null, ex, nameof(EndSend));
            }
        }


        //
        // Receive Side
        //

        // Both Receive and Receive(timeout) should fail with a 
        // if we get a timeout, we have no way of knowing which
        // outstanding message it closed :(
        public Message Receive()
        {
            var response = DuplexChannel.Receive();
            if ( response != null )
            {
                HandleReply(response);
            }
            return response;
        }

        public Message Receive(TimeSpan timeout)
        {
            var response = DuplexChannel.Receive(timeout);
            if ( response != null )
            {
                HandleReply(response);
            }
            return response;
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            bool success = DuplexChannel.TryReceive(timeout, out message);
            if ( success && message != null )
            {
                HandleReply(message);
            }
            return success;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return DuplexChannel.WaitForMessage(timeout);
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return DuplexChannel.BeginReceive(callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return DuplexChannel.BeginReceive(timeout, callback, state);
        }
        public Message EndReceive(IAsyncResult result)
        {
            var reply = DuplexChannel.EndReceive(result);
            if ( reply != null )
            {
                this.HandleReply(reply);
            }
            return reply;
        }


        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return DuplexChannel.BeginTryReceive(timeout, callback, state);
        }
        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            bool success = DuplexChannel.EndTryReceive(result, out message);
            if ( success && message != null )
            {
                this.HandleReply(message);
            }
            return success;
        }


        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return DuplexChannel.BeginWaitForMessage(timeout, callback, state);
        }
        public bool EndWaitForMessage(IAsyncResult result)
        {
            return DuplexChannel.EndWaitForMessage(result);
        }


        protected override void OnClose()
        {
            correlator.Clear();
            base.OnClose();
        }
        private void HandleReply(Message reply)
        {
            DependencyTelemetry telemetry = null;
            if ( correlator.TryLookup(reply.Headers.RelatesTo, out telemetry) )
            {
                StopSendTelemetry(telemetry, reply, null, nameof(HandleReply));
            }
            // not our message, leave it be
        }
    }
}
