namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;

    internal sealed class RequestAsyncResult : ChannelAsyncResult
    {
        public RequestAsyncResult(IRequestChannel innerChannel, Message message, TimeSpan timeout, AsyncCallback onRequestDone, AsyncCallback callback, object state, DependencyTelemetry telemetry)
            : base(onRequestDone, callback, state, telemetry)
        {
            this.InnerChannel = innerChannel;

            this.OriginalResult = innerChannel.BeginRequest(message, timeout, OnComplete, this);
            if (this.OriginalResult.CompletedSynchronously)
            {
                this.Reply = innerChannel.EndRequest(this.OriginalResult);
                this.Complete(true);
            }
        }

        public IRequestChannel InnerChannel { get; private set; }

        public Message Reply { get; private set; }

        private static void OnComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            var rar = (RequestAsyncResult)result.AsyncState;
            try
            {
                rar.Reply = rar.InnerChannel.EndRequest(rar.OriginalResult);
                rar.Complete(false);
            }
            catch (Exception ex)
            {
                rar.Complete(false, ex);
            }
        }
    }
}
