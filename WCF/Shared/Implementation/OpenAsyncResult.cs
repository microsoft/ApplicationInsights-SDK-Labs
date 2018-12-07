namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;

    internal sealed class OpenAsyncResult : ChannelAsyncResult
    {
        public OpenAsyncResult(IChannel innerChannel, TimeSpan timeout, AsyncCallback onOpenDone, AsyncCallback callback, object state, DependencyTelemetry telemetry)
            : base(onOpenDone, callback, state, telemetry)
        {
            this.InnerChannel = innerChannel;

            this.OriginalResult = innerChannel.BeginOpen(timeout, OnComplete, this);
            if (this.OriginalResult.CompletedSynchronously)
            {
                innerChannel.EndOpen(this.OriginalResult);
                this.CompleteSynchronously();
            }
        }

        public IChannel InnerChannel { get; private set; }

        private static void OnComplete(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            var oar = (OpenAsyncResult)result.AsyncState;
            try
            {
                oar.InnerChannel.EndOpen(oar.OriginalResult);
                oar.Complete(false);
            }
            catch (Exception ex)
            {
                oar.Complete(false, ex);
            }
        }
    }
}
