namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Channels;

    internal sealed class TryReceiveAsyncResult : ChannelAsyncResult
    {
        public TryReceiveAsyncResult(IInputChannel innerChannel, TimeSpan timeout, AsyncCallback onReceiveDone, AsyncCallback callback, object state)
            : base(onReceiveDone, callback, state, null)
        {
            this.InnerChannel = innerChannel;

            this.OriginalResult = innerChannel.BeginTryReceive(timeout, OnComplete, this);
            if (this.OriginalResult.CompletedSynchronously)
            {
                Message message = null;
                this.Result = innerChannel.EndTryReceive(this.OriginalResult, out message);
                this.Message = message;
                this.Complete(true);
            }
        }

        public IInputChannel InnerChannel { get; private set; }

        public Message Message { get; private set; }

        public bool Result { get; private set; }

        private static void OnComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            var trar = (TryReceiveAsyncResult)result.AsyncState;
            try
            {
                Message message = null;
                trar.Result = trar.InnerChannel.EndTryReceive(trar.OriginalResult, out message);
                trar.Message = message;
                trar.Complete(false);
            }
            catch (Exception ex)
            {
                trar.Complete(false, ex);
            }
        }
    }
}
