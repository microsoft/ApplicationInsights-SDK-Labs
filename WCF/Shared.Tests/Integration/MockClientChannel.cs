using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    class MockClientChannel : IRequestChannel, IOutputChannel, IDuplexChannel
    {
        public EndpointAddress LocalAddress { get; private set; }

        public TimeSpan OperationTimeout { get; set; }

        public IOutputSession OutputSession { get; private set; }

        public EndpointAddress RemoteAddress { get; private set; }

        public CommunicationState State { get; private set; }

        public Uri Via { get; private set; }

        public event EventHandler Closed;
        public event EventHandler Closing;
        public event EventHandler Faulted;
        public event EventHandler Opened;
        public event EventHandler Opening;
        public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived { add { } remove { } }

        public bool FailOpen { get; set; }
        public bool FailBeginOpen { get; set; }
        public bool ReturnSoapFault { get; set; }
        public bool FailRequest { get; set; }
        public bool FailEndRequest { get; set; }
        public Message LastMessageSent { get; private set; }
        public Message MessageToReceive { get; set; }


        public MockClientChannel(String remoteUrl)
        {
            this.RemoteAddress = new EndpointAddress(remoteUrl);
            ChangeState(CommunicationState.Created);
        }

        public void Open()
        {
            SimulateOpen(TimeSpan.MaxValue, FailOpen);
            if ( FailOpen )
            {
                throw new EndpointNotFoundException();
            }
        }
        public void Open(TimeSpan timeout)
        {
            SimulateOpen(TimeSpan.Zero, FailOpen);
            if ( FailOpen )
            {
                throw new EndpointNotFoundException();
            }
        }
        public void SimulateOpen(TimeSpan duration, bool fail)
        {
            ChangeState(CommunicationState.Opening);
            if ( !fail )
            {
                ChangeState(CommunicationState.Opened);
            } else
            {
                ChangeState(CommunicationState.Faulted);
            }
        }

        public void Close()
        {
            Close(TimeSpan.MaxValue);
        }

        public void Close(TimeSpan timeout)
        {
            ChangeState(CommunicationState.Closing);
            ChangeState(CommunicationState.Closed);
        }

        public void Abort()
        {
            Close();
        }

        public void Dispose()
        {
            Close();
        }

        public bool OpeningIsHooked()
        {
            return Opening != null && Opening.GetInvocationList().Length > 0;
        }
        public bool OpenedIsHooked()
        {
            return Opened != null && Opened.GetInvocationList().Length > 0;
        }
        public bool ClosingIsHooked()
        {
            return Closing != null && Closing.GetInvocationList().Length > 0;
        }
        public bool ClosedIsHooked()
        {
            return Closed != null && Closed.GetInvocationList().Length > 0;
        }
        public bool FaultedIsHooked()
        {
            return Faulted != null && Faulted.GetInvocationList().Length > 0;
        }

        private void ChangeState(CommunicationState state)
        {
            this.State = state;
            switch ( state )
            {
            case CommunicationState.Opening:
                Opening?.Invoke(this, EventArgs.Empty);
                break;
            case CommunicationState.Opened:
                Opened?.Invoke(this, EventArgs.Empty);
                break;
            case CommunicationState.Closing:
                Closing?.Invoke(this, EventArgs.Empty);
                break;
            case CommunicationState.Closed:
                Closed?.Invoke(this, EventArgs.Empty);
                break;
            case CommunicationState.Faulted:
                Faulted?.Invoke(this, EventArgs.Empty);
                break;
            }
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            ChangeState(CommunicationState.Opening);
            if ( FailBeginOpen )
            {
                ChangeState(CommunicationState.Faulted);
                throw new EndpointNotFoundException();
            }
            return new SyncAsyncResult(callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ChangeState(CommunicationState.Opening);
            if ( FailBeginOpen )
            {
                ChangeState(CommunicationState.Faulted);
                throw new EndpointNotFoundException();
            }
            return new SyncAsyncResult(callback, state);
        }

        public void EndOpen(IAsyncResult result)
        {
            ((SyncAsyncResult)result).End();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            ((SyncAsyncResult)result).End();
        }

        public T GetProperty<T>() where T : class
        {
            return default(T);
        }

        // request channel methods
        public Message Request(Message message)
        {
            return Request(message, TimeSpan.FromSeconds(10));
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            if ( FailRequest)
            {
                throw new TimeoutException();
            }
            return BuildMessage(message.Headers.Action);
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return BeginRequest(message, TimeSpan.FromSeconds(10), callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            LastMessageSent = message;
            if ( FailRequest )
            {
                throw new TimeoutException();
            }
            return new SyncAsyncResult(callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            ((SyncAsyncResult)result).End();
            if ( FailEndRequest )
            {
                throw new TimeoutException();
            }
            return BuildMessage("*");
        }


        //
        // Output Channel Impl
        //
        public void Send(Message message)
        {
            Send(message, TimeSpan.FromSeconds(10));
        }

        public void Send(Message message, TimeSpan timeout)
        {
            LastMessageSent = message;
            if ( FailRequest )
            {
                throw new TimeoutException();
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return BeginSend(message, TimeSpan.FromSeconds(10), callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            LastMessageSent = message;
            if ( FailRequest )
            {
                throw new TimeoutException();
            }
            return new SyncAsyncResult(callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            if ( FailEndRequest )
            {
                throw new TimeoutException();
            }
            ((SyncAsyncResult)result).End();
        }


        private Message BuildMessage(String action)
        {
            if ( ReturnSoapFault )
            {
                return BuildFaultMessage(action);
            }
            return Message.CreateMessage(MessageVersion.Default, action, "<text/>");
        }
        private Message BuildFaultMessage(String action)
        {
            return Message.CreateMessage(
                MessageVersion.Default,
                MessageFault.CreateFault(
                    FaultCode.CreateReceiverFaultCode("e1", "http://tempuri.org"),
                    "There was an error processing the message"
                    ),
                action);
        }

        //
        //
        // IDuplexChannel fields
        //
        public Message Receive()
        {
            if ( MessageToReceive != null )
            {
                return MessageToReceive;
            }
            throw new TimeoutException();
        }

        public Message Receive(TimeSpan timeout)
        {
            if ( MessageToReceive != null )
            {
                return MessageToReceive;
            }
            throw new TimeoutException();
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(callback, state);
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(callback, state);
        }

        public Message EndReceive(IAsyncResult result)
        {
            ((SyncAsyncResult)result).End();
            if ( MessageToReceive != null )
            {
                return MessageToReceive;
            }
            throw new TimeoutException();
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            message = MessageToReceive;
            return message != null;
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            ((SyncAsyncResult)result).End();
            message = MessageToReceive;
            return message != null;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return MessageToReceive != null;
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            ((SyncAsyncResult)result).End();
            return MessageToReceive != null;
        }

        class SyncAsyncResult : IAsyncResult
        {
            private EventWaitHandle waitHandle;
            private AsyncCallback callback;
            private Timer timer;
            public object AsyncState { get; private set; }

            public WaitHandle AsyncWaitHandle { get { return waitHandle; } }

            public bool CompletedSynchronously { get; private set; }

            public bool IsCompleted { get; private set; }

            public SyncAsyncResult(AsyncCallback callback, object state)
            {
                this.AsyncState = state;
                this.callback = callback;
                waitHandle = new ManualResetEvent(false);
                CompletedSynchronously = false;
                IsCompleted = false;
                timer = new Timer(this.OnDone, null, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(-1));
            }

            public void End()
            {
                this.waitHandle.WaitOne();
                this.waitHandle.Close();
            }
            private void Complete()
            {
                this.IsCompleted = true;
                this.waitHandle.Set();
                callback?.Invoke(this);
            }

            private void OnDone(object state)
            {
                this.Complete();
            }
        }
    }
}
