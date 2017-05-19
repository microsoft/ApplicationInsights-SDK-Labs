namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Threading;
    using Microsoft.ApplicationInsights.DataContracts;

    internal abstract class ChannelAsyncResult : IAsyncResult, IDisposable
    {
        private const int Incomplete = 0;
        private const int CompletedSync = 1;
        private const int CompletedAsync = 2;
        private AsyncCallback callback;
        private EventWaitHandle waitHandle;
        private AsyncCallback channelCompletionCallback;
        private object lockObj;
        private int completed;

        public ChannelAsyncResult(AsyncCallback completeCallback, AsyncCallback callback, object state, DependencyTelemetry channelState)
        {
            this.AsyncState = state;
            this.lockObj = new object();
            this.waitHandle = null;
            this.Telemetry = channelState;
            this.channelCompletionCallback = completeCallback;
            this.callback = callback;
        }

        public object AsyncState { get; private set; }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (this.waitHandle == null)
                {
                    lock (this.lockObj)
                    {
                        if (this.waitHandle == null)
                        {
                            this.waitHandle = new EventWaitHandle(this.IsCompleted, EventResetMode.ManualReset);
                        }
                    }
                }

                return this.waitHandle;
            }
        }

        public bool CompletedSynchronously
        {
            get { return Thread.VolatileRead(ref this.completed) == CompletedSync; }
        }

        public bool IsCompleted
        {
            get { return Thread.VolatileRead(ref this.completed) != Incomplete; }
        }

        // should only be read once we're complete and we don't care if
        // it's not read in order
        public Exception LastException { get; private set; }

        // these get set during construction, so no need for much check
        public DependencyTelemetry Telemetry { get; private set; }

        public IAsyncResult OriginalResult { get; protected set; }

        public static TAsyncResult End<TAsyncResult>(IAsyncResult result) where TAsyncResult : ChannelAsyncResult
        {
            var car = (ChannelAsyncResult)result;
            if (!car.IsCompleted)
            {
                car.AsyncWaitHandle.WaitOne();
            }

            ((IDisposable)car).Dispose();

            if (car.LastException != null)
            {
                throw car.LastException;
            }

            return (TAsyncResult)car;
        }

        void IDisposable.Dispose()
        {
            // done here only to avoid CA1001
            if (this.waitHandle != null)
            {
                this.waitHandle.Close();
            }
        }

        protected void CompleteSynchronously()
        {
            try
            {
                this.Complete(true);
            }
            finally
            {
                ((IDisposable)this).Dispose();
            }
        }

        protected void Complete(bool completedSync, Exception exception = null)
        {
            this.LastException = exception;

            Thread.VolatileWrite(ref this.completed, completedSync ? CompletedSync : CompletedAsync);

            try
            {
                // tell channel the async operation is done
                this.NotifyCompletionToChannel();
            }
            catch (Exception ex)
            {
                this.LastException = ex;
            }

            // set the waitHandle so that when callback() calls EndWhatever()
            // it doesn't hang
            if (this.waitHandle != null)
            {
                this.waitHandle.Set();
            }

            try
            {
                this.callback?.Invoke(this);
            }
            catch (Exception ex)
            {
                this.LastException = ex;
            }
        }

        protected void NotifyCompletionToChannel()
        {
            this.channelCompletionCallback?.Invoke(this);
        }
    }
}
