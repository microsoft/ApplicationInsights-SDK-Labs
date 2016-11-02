using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    public class TraceTelemetryModule : IWcfTelemetryModule, IWcfMessageTrace
    {
        private static bool enabled = false;
        private TelemetryClient client;

        public void Initialize(TelemetryConfiguration configuration)
        {
            client = new TelemetryClient(configuration);
        }

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
            if ( enabled )
            {
                EventTelemetry ev = new EventTelemetry("WcfRequest");
                ev.Properties.Add("Body", ReadMessageBody(ref request));
                client.TrackEvent(ev);
            }
        }

        public void OnTraceResponse(IOperationContext operation, ref Message response)
        {
            if ( enabled )
            {
                EventTelemetry ev = new EventTelemetry("WcfResponse");
                ev.Properties.Add("Body", ReadMessageBody(ref response));
                client.TrackEvent(ev);
            }
        }

        private String ReadMessageBody(ref Message msg)
        {
            var buffer = msg.CreateBufferedCopy(int.MaxValue);
            var result = "";
            using ( msg = buffer.CreateMessage() )
            {
                using ( var reader = msg.GetReaderAtBodyContents() )
                {
                    result = reader.ReadOuterXml();
                }
            }
            msg = buffer.CreateMessage();
            return result;
        }
    }
}
