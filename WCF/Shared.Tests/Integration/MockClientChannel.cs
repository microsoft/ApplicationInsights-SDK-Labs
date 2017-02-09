using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    class MockClientChannel : IClientChannel, IRequestChannel
    {
        public bool AllowInitializationUI { get; set; }

        public bool AllowOutputBatching { get; set; }

        public bool DidInteractiveInitialization { get; private set; }

        public IExtensionCollection<IContextChannel> Extensions { get; private set; }

        public IInputSession InputSession { get; private set; }

        public EndpointAddress LocalAddress { get; private set; }

        public TimeSpan OperationTimeout { get; set; }

        public IOutputSession OutputSession { get; private set; }

        public EndpointAddress RemoteAddress { get; private set; }

        public string SessionId { get; private set; }

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
            return new SyncAsyncResult(state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ChangeState(CommunicationState.Opening);
            if ( FailBeginOpen )
            {
                ChangeState(CommunicationState.Faulted);
                throw new EndpointNotFoundException();
            }
            return new SyncAsyncResult(state);
        }

        public void EndOpen(IAsyncResult result)
        {
            result.AsyncWaitHandle.WaitOne();
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(state);
        }

        public void EndClose(IAsyncResult result)
        {
            result.AsyncWaitHandle.WaitOne();
        }

        public IAsyncResult BeginDisplayInitializationUI(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }


        public void DisplayInitializationUI()
        {
            throw new NotImplementedException();
        }

        public void EndDisplayInitializationUI(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public T GetProperty<T>() where T : class
        {
            throw new NotImplementedException();
        }

        // request channel methods
        public Message Request(Message message)
        {
            if ( FailRequest)
            {
                throw new TimeoutException();
            }
            return BuildMessage(message.Headers.Action);
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
            if ( FailRequest )
            {
                throw new TimeoutException();
            }
            return new SyncAsyncResult(state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if ( FailRequest )
            {
                throw new TimeoutException();
            }
            return new SyncAsyncResult(state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            result.AsyncWaitHandle.WaitOne();
            if ( FailEndRequest )
            {
                throw new TimeoutException();
            }
            return BuildMessage("*");
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


        class SyncAsyncResult : IAsyncResult
        {
            public object AsyncState { get; private set; }

            public WaitHandle AsyncWaitHandle { get; private set; }

            public bool CompletedSynchronously { get; private set; }

            public bool IsCompleted { get; private set; }

            public SyncAsyncResult(object state)
            {
                this.AsyncState = state;
                AsyncWaitHandle = new ManualResetEvent(true);
                CompletedSynchronously = true;
                IsCompleted = true;
            }
        }
    }
}
