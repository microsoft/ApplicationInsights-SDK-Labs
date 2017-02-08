using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    class MockClientChannel : IClientChannel
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


        public MockClientChannel(String remoteUrl)
        {
            this.RemoteAddress = new EndpointAddress(remoteUrl);
            ChangeState(CommunicationState.Created);
        }

        public void Open()
        {
            Open(TimeSpan.MaxValue);
        }
        public void Open(TimeSpan timeout)
        {
            SimulateOpen(TimeSpan.Zero, false);
        }
        public void SimulateOpen(TimeSpan duration, bool fail)
        {
            ChangeState(CommunicationState.Opening);
            // ugly, ugly, ugly
            System.Threading.Thread.Sleep(duration);
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



        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginDisplayInitializationUI(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void DisplayInitializationUI()
        {
            throw new NotImplementedException();
        }

        public void EndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public void EndDisplayInitializationUI(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public void EndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public T GetProperty<T>() where T : class
        {
            throw new NotImplementedException();
        }

    }
}
