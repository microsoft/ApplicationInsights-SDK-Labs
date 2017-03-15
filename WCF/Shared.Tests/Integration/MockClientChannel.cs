namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    internal class MockClientChannel : IRequestChannel, IOutputChannel, IDuplexChannel
    {
        public MockClientChannel(string remoteUrl)
        {
            this.RemoteAddress = new EndpointAddress(remoteUrl);
            this.ChangeState(CommunicationState.Created);
        }

        public event EventHandler Closed;

        public event EventHandler Closing;

        public event EventHandler Faulted;

        public event EventHandler Opened;

        public event EventHandler Opening;

        public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived
        {
            add { }
            remove { }
        }

        public EndpointAddress LocalAddress { get; private set; }

        public TimeSpan OperationTimeout { get; set; }

        public IOutputSession OutputSession { get; private set; }

        public EndpointAddress RemoteAddress { get; private set; }

        public CommunicationState State { get; private set; }

        public Uri Via { get; private set; }

        public bool FailOpen { get; set; }

        public bool FailBeginOpen { get; set; }

        public bool ReturnSoapFault { get; set; }

        public bool FailRequest { get; set; }

        public bool FailEndRequest { get; set; }

        public Message LastMessageSent { get; private set; }

        public Message MessageToReceive { get; set; }

        public Exception ExceptionToThrowOnSend { get; set; }

        public void Open()
        {
            this.Open(TimeSpan.MaxValue);
        }

        public void Open(TimeSpan timeout)
        {
            this.SimulateOpen(TimeSpan.Zero, this.FailOpen);
            if (this.FailOpen)
            {
                throw new EndpointNotFoundException();
            }
        }

        public void SimulateOpen(TimeSpan duration, bool fail)
        {
            this.ChangeState(CommunicationState.Opening);
            if (!fail)
            {
                this.ChangeState(CommunicationState.Opened);
            }
            else
            {
                this.ChangeState(CommunicationState.Faulted);
            }
        }

        public void Close()
        {
            this.Close(TimeSpan.MaxValue);
        }

        public void Close(TimeSpan timeout)
        {
            this.ChangeState(CommunicationState.Closing);
            this.ChangeState(CommunicationState.Closed);
        }

        public void Abort()
        {
            this.Close();
        }

        public void Dispose()
        {
            this.Close();
        }

        public bool OpeningIsHooked()
        {
            return this.Opening != null && this.Opening.GetInvocationList().Length > 0;
        }

        public bool OpenedIsHooked()
        {
            return this.Opened != null && this.Opened.GetInvocationList().Length > 0;
        }

        public bool ClosingIsHooked()
        {
            return this.Closing != null && this.Closing.GetInvocationList().Length > 0;
        }

        public bool ClosedIsHooked()
        {
            return this.Closed != null && this.Closed.GetInvocationList().Length > 0;
        }

        public bool FaultedIsHooked()
        {
            return this.Faulted != null && this.Faulted.GetInvocationList().Length > 0;
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return this.BeginOpen(TimeSpan.MaxValue, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ChangeState(CommunicationState.Opening);
            if (this.FailBeginOpen)
            {
                this.ChangeState(CommunicationState.Faulted);
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
            return this.Request(message, TimeSpan.FromSeconds(10));
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            if (this.FailRequest)
            {
                throw new TimeoutException();
            }

            return this.BuildMessage(message.Headers.Action);
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, TimeSpan.FromSeconds(10), callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.LastMessageSent = message;
            if (this.FailRequest)
            {
                if (this.ExceptionToThrowOnSend != null)
                {
                    throw this.ExceptionToThrowOnSend;
                }

                throw new TimeoutException();
            }

            return new SyncAsyncResult(callback, state);
        }

        public Message EndRequest(IAsyncResult result)
        {
            ((SyncAsyncResult)result).End();
            if (this.FailEndRequest)
            {
                throw new TimeoutException();
            }

            return this.BuildMessage("*");
        }

        // -----------------------------------
        // Output Channel Impl
        // -----------------------------------
        public void Send(Message message)
        {
            this.Send(message, TimeSpan.FromSeconds(10));
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.LastMessageSent = message;
            if (this.FailRequest)
            {
                if (this.ExceptionToThrowOnSend != null)
                {
                    throw this.ExceptionToThrowOnSend;
                }

                throw new TimeoutException();
            }
        }

        public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
        {
            return this.BeginSend(message, TimeSpan.FromSeconds(10), callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.LastMessageSent = message;
            if (this.FailRequest)
            {
                throw new TimeoutException();
            }

            return new SyncAsyncResult(callback, state);
        }

        public void EndSend(IAsyncResult result)
        {
            if (this.FailEndRequest)
            {
                throw new TimeoutException();
            }

            ((SyncAsyncResult)result).End();
        }

        // -------------------------------------------
        // IDuplexChannel fields
        // -------------------------------------------
        public Message Receive()
        {
            if (this.MessageToReceive != null)
            {
                return this.MessageToReceive;
            }

            throw new TimeoutException();
        }

        public Message Receive(TimeSpan timeout)
        {
            if (this.MessageToReceive != null)
            {
                return this.MessageToReceive;
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
            if (this.MessageToReceive != null)
            {
                return this.MessageToReceive;
            }

            throw new TimeoutException();
        }

        public bool TryReceive(TimeSpan timeout, out Message message)
        {
            message = this.MessageToReceive;
            return message != null;
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(callback, state);
        }

        public bool EndTryReceive(IAsyncResult result, out Message message)
        {
            ((SyncAsyncResult)result).End();
            message = this.MessageToReceive;
            return message != null;
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.MessageToReceive != null;
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new SyncAsyncResult(callback, state);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            ((SyncAsyncResult)result).End();
            return this.MessageToReceive != null;
        }

        private void ChangeState(CommunicationState state)
        {
            this.State = state;
            switch (state)
            {
                case CommunicationState.Opening:
                    this.Opening?.Invoke(this, EventArgs.Empty);
                    break;
                case CommunicationState.Opened:
                    this.Opened?.Invoke(this, EventArgs.Empty);
                    break;
                case CommunicationState.Closing:
                    this.Closing?.Invoke(this, EventArgs.Empty);
                    break;
                case CommunicationState.Closed:
                    this.Closed?.Invoke(this, EventArgs.Empty);
                    break;
                case CommunicationState.Faulted:
                    this.Faulted?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }

        private Message BuildMessage(string action)
        {
            if (this.ReturnSoapFault)
            {
                return this.BuildFaultMessage(action);
            }

            return Message.CreateMessage(MessageVersion.Default, action, "<text/>");
        }

        private Message BuildFaultMessage(string action)
        {
            var fault = MessageFault.CreateFault(
                    FaultCode.CreateReceiverFaultCode("e1", "http://tempuri.org"),
                    "There was an error processing the message");
            return Message.CreateMessage(MessageVersion.Default, fault, action);
        }

        private class SyncAsyncResult : IAsyncResult, IDisposable
        {
            private EventWaitHandle waitHandle;
            private AsyncCallback callback;
            private Timer timer;

            public SyncAsyncResult(AsyncCallback callback, object state)
            {
                this.AsyncState = state;
                this.callback = callback;
                this.waitHandle = new ManualResetEvent(false);
                this.CompletedSynchronously = false;
                this.IsCompleted = false;
                this.timer = new Timer(this.OnDone, null, TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(-1));
            }

            public object AsyncState { get; private set; }

            public WaitHandle AsyncWaitHandle
            {
                get { return this.waitHandle; }
            }

            public bool CompletedSynchronously { get; private set; }

            public bool IsCompleted { get; private set; }

            public void End()
            {
                this.waitHandle.WaitOne();
                this.waitHandle.Close();
                this.timer.Dispose();
            }

            void IDisposable.Dispose()
            {
                this.End();
            }

            private void Complete()
            {
                this.IsCompleted = true;
                this.waitHandle.Set();
                this.callback?.Invoke(this);
            }

            private void OnDone(object state)
            {
                this.Complete();
            }
        }
    }
}
