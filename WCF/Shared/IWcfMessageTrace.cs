using System;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Represents a telemetry module that also is interested
    /// in tracing request messages before the request is executed.
    /// </summary>
    public interface IWcfMessageTrace
    {
        /// <summary>
        /// Called after IWcfTelemetryModule.OnBeginRequest()
        /// but before the request is executed.
        /// </summary>
        /// <param name="operation">The operation context</param>
        /// <param name="request">The request message. You can modify the request by returning a new, unread Message object.</param>
        void OnTraceRequest(IOperationContext operation, ref Message request);

        /// <summary>
        /// Called after the request is executed, but before
        /// IWcfTelemetryModule.OnEndRequest() is called.
        /// </summary>
        /// <param name="operation">The operation context</param>
        /// <param name="response">The response message. You can modify the response by returning a new, unread Message object.</param>
        void OnTraceResponse(IOperationContext operation, ref Message response);
    }
}
