using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal abstract class ChannelAsyncResult : IAsyncResult, IDisposable
    {
        private AsyncCallback callback;
        private EventWaitHandle waitHandle;
        private AsyncCallback channelCompletionCallback;
        const int Incomplete = 0;
        const int CompletedSync = 1;
        const int CompletedAsync = 2;
        private int completed;

        public object AsyncState { get; private set; }

        public WaitHandle AsyncWaitHandle { get { return waitHandle; } }

        public bool CompletedSynchronously
        {
            get { return Thread.VolatileRead(ref completed) == CompletedSync; }
        }

        public bool IsCompleted
        {
            get { return Thread.VolatileRead(ref completed) != Incomplete; }
        }
        // should only be read once we're complete and we don't care if
        // it's not read in order
        public Exception LastException { get; private set; }

        // these get set during construction, so no need for much check
        public DependencyTelemetry Telemetry { get; private set; }
        public IAsyncResult OriginalResult { get; protected set; }

        public ChannelAsyncResult(AsyncCallback completeCallback, AsyncCallback callback, object state, DependencyTelemetry channelState)
        {
            this.AsyncState = state;
            this.waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.Telemetry = channelState;
            this.channelCompletionCallback = completeCallback;
            this.callback = callback;
        }

        protected void Complete(bool completedSync, Exception exception = null)
        {
            this.LastException = exception;

            Thread.VolatileWrite(ref this.completed, completedSync ? CompletedSync : CompletedAsync);

            try
            {
                // tell channel the async operation is done
                NotifyCompletionToChannel();
            } catch ( Exception ex )
            {
                LastException = ex;
            }
            // set the waitHandle so that when callback() calls EndWhatever()
            // it doesn't hang
            this.waitHandle.Set();
            try
            {
                callback?.Invoke(this);
            } catch ( Exception ex )
            {
                LastException = ex;
            }
        }

        protected void NotifyCompletionToChannel()
        {
            this.channelCompletionCallback?.Invoke(this);
        }

        public static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : ChannelAsyncResult
        {
            ChannelAsyncResult car = (ChannelAsyncResult)result;
            if ( !car.IsCompleted )
            {
                car.AsyncWaitHandle.WaitOne();
            }

            ((IDisposable)car).Dispose();

            if ( car.LastException != null )
            {
                throw car.LastException;
            }
            return (TAsyncResult)car;
        }

        void IDisposable.Dispose()
        {
            // done here only to avoid CA1001
            if ( this.waitHandle != null )
            {
                this.waitHandle.Close();
            }
        }
    }

    internal sealed class OpenAsyncResult : ChannelAsyncResult
    {
        public IChannel InnerChannel { get; private set; }

        public OpenAsyncResult(IChannel innerChannel, TimeSpan timeout, AsyncCallback onOpenDone, AsyncCallback callback, object state, DependencyTelemetry telemetry)
            : base(onOpenDone, callback, state, telemetry)
        {
            this.InnerChannel = innerChannel;

            OriginalResult = innerChannel.BeginOpen(timeout, OnComplete, this);
            if ( OriginalResult.CompletedSynchronously )
            {
                innerChannel.EndOpen(OriginalResult);
                this.Complete(true);
            }
        }

        private static void OnComplete(IAsyncResult result)
        {
            if ( result.CompletedSynchronously )
            {
                return;
            }
            OpenAsyncResult oar = (OpenAsyncResult)result.AsyncState;
            try
            {
                oar.InnerChannel.EndOpen(oar.OriginalResult);
                oar.Complete(false);
            } catch ( Exception ex )
            {
                oar.Complete(false, ex);
            }
        }
    }

    internal sealed class SendAsyncResult : ChannelAsyncResult
    {
        public IOutputChannel InnerChannel { get; private set; }
        public UniqueId RequestId { get; private set; }

        public SendAsyncResult(IOutputChannel innerChannel, Message message, TimeSpan timeout, AsyncCallback onSendDone, AsyncCallback callback, object state, DependencyTelemetry telemetry)
            : base(onSendDone, callback, state, telemetry)
        {
            this.InnerChannel = innerChannel;
            this.RequestId = message.Headers.MessageId;

            this.OriginalResult = innerChannel.BeginSend(message, timeout, OnComplete, this);
            if ( OriginalResult.CompletedSynchronously )
            {
                innerChannel.EndSend(OriginalResult);
                this.Complete(true);
            }
        }

        private static void OnComplete(IAsyncResult result)
        {
            if ( result.CompletedSynchronously )
            {
                return;
            }
            SendAsyncResult sar = (SendAsyncResult)result.AsyncState;
            try
            {
                sar.InnerChannel.EndSend(sar.OriginalResult);
                sar.Complete(false);
            } catch ( Exception ex )
            {
                sar.Complete(false, ex);
            }
        }
    }

    internal sealed class RequestAsyncResult : ChannelAsyncResult
    {
        public IRequestChannel InnerChannel { get; private set; }
        public Message Reply { get; private set; }

        public RequestAsyncResult(IRequestChannel innerChannel, Message message, TimeSpan timeout, AsyncCallback onRequestDone, AsyncCallback callback, object state, DependencyTelemetry telemetry)
            : base(onRequestDone, callback, state, telemetry)
        {
            this.InnerChannel = innerChannel;

            OriginalResult = innerChannel.BeginRequest(message, timeout, OnComplete, this);
            if ( OriginalResult.CompletedSynchronously )
            {
                this.Reply = innerChannel.EndRequest(OriginalResult);
                this.Complete(true);
            }
        }

        private static void OnComplete(IAsyncResult result)
        {
            if ( result.CompletedSynchronously )
            {
                return;
            }
            RequestAsyncResult rar = (RequestAsyncResult)result.AsyncState;
            try
            {
                rar.Reply = rar.InnerChannel.EndRequest(rar.OriginalResult);
                rar.Complete(false);
            } catch ( Exception ex )
            {
                rar.Complete(false, ex);
            }
        }
    }

    internal sealed class ReceiveAsyncResult : ChannelAsyncResult
    {
        public IInputChannel InnerChannel { get; private set; }
        public Message Message { get; private set; }

        public ReceiveAsyncResult(IInputChannel innerChannel, TimeSpan timeout, AsyncCallback onReceiveDone, AsyncCallback callback, object state)
            : base(onReceiveDone, callback, state, null)
        {
            this.InnerChannel = innerChannel;

            OriginalResult = innerChannel.BeginReceive(timeout, OnComplete, this);
            if ( OriginalResult.CompletedSynchronously )
            {
                this.Message = innerChannel.EndReceive(OriginalResult);
                this.Complete(true);
            }
        }

        private static void OnComplete(IAsyncResult result)
        {
            if ( result.CompletedSynchronously )
            {
                return;
            }
            ReceiveAsyncResult rar = (ReceiveAsyncResult)result.AsyncState;
            try
            {
                rar.Message = rar.InnerChannel.EndReceive(rar.OriginalResult);
                rar.Complete(false);
            } catch ( Exception ex )
            {
                rar.Complete(false, ex);
            }
        }
    }

    internal sealed class TryReceiveAsyncResult : ChannelAsyncResult
    {
        public IInputChannel InnerChannel { get; private set; }
        public Message Message { get; private set; }
        public bool Result { get; private set; }

        public TryReceiveAsyncResult(IInputChannel innerChannel, TimeSpan timeout, AsyncCallback onReceiveDone, AsyncCallback callback, object state)
            : base(onReceiveDone, callback, state, null)
        {
            this.InnerChannel = innerChannel;

            OriginalResult = innerChannel.BeginTryReceive(timeout, OnComplete, this);
            if ( OriginalResult.CompletedSynchronously )
            {
                Message message = null;
                this.Result = innerChannel.EndTryReceive(OriginalResult, out message);
                this.Message = message;
                this.Complete(true);
            }
        }

        private static void OnComplete(IAsyncResult result)
        {
            if ( result.CompletedSynchronously )
            {
                return;
            }
            TryReceiveAsyncResult trar = (TryReceiveAsyncResult)result.AsyncState;
            try
            {
                Message message = null;
                trar.Result = trar.InnerChannel.EndTryReceive(trar.OriginalResult, out message);
                trar.Message = message;
                trar.Complete(false);
            } catch ( Exception ex )
            {
                trar.Complete(false, ex);
            }
        }
    }
}
