namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;

    public class TraceTelemetryModule : IWcfTelemetryModule, IWcfMessageTrace
    {
        private static bool enabled = false;
        private TelemetryClient client;

        // needed to avoid breaking other tests
        // by tracking events that are not expected
        public static void Enable()
        {
            enabled = true;
        }

        public static void Disable()
        {
            enabled = false;
        }

        public void Initialize(TelemetryConfiguration configuration)
        {
            this.client = new TelemetryClient(configuration);
        }

        public void OnBeginRequest(IOperationContext operation)
        {
        }

        public void OnEndRequest(IOperationContext operation, Message reply)
        {
        }

        public void OnError(IOperationContext operation, Exception error)
        {
        }

        public void OnTraceRequest(IOperationContext operation, ref Message request)
        {
            if (enabled)
            {
                EventTelemetry ev = new EventTelemetry("WcfRequest");
                ev.Properties.Add("Body", ReadMessageBody(ref request));
                this.client.TrackEvent(ev);
            }
        }

        public void OnTraceResponse(IOperationContext operation, ref Message response)
        {
            if (enabled)
            {
                EventTelemetry ev = new EventTelemetry("WcfResponse");
                ev.Properties.Add("Body", ReadMessageBody(ref response));
                this.client.TrackEvent(ev);
            }
        }

        private static string ReadMessageBody(ref Message msg)
        {
            var buffer = msg.CreateBufferedCopy(int.MaxValue);
            var result = string.Empty;
            using (msg = buffer.CreateMessage())
            {
                using (var reader = msg.GetReaderAtBodyContents())
                {
                    result = reader.ReadOuterXml();
                }
            }

            msg = buffer.CreateMessage();
            return result;
        }
    }
}
