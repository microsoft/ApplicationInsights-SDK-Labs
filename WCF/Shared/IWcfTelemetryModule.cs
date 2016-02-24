using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Represents a telemetry module used during WCF processing
    /// </summary>
    public interface IWcfTelemetryModule : ITelemetryModule
    {
        /// <summary>
        /// Fired when the request message arrives
        /// </summary>
        /// <param name="operation">The operation context</param>
        /// <param name="request">The service request</param>
        void OnBeginRequest(IOperationContext operation, IRequestHolder request);
        /// <summary>
        /// Fired before the response message is sent
        /// </summary>
        /// <param name="operation">The operation context</param>
        /// <param name="request">The service request</param>
        /// <param name="reply">The response message</param>
        void OnEndRequest(IOperationContext operation, IRequestHolder request, Message reply);
        /// <summary>
        /// Fired when an exception occurs
        /// </summary>
        /// <param name="operation">The operation context</param>
        /// <param name="error">The exception object</param>
        void OnError(IOperationContext operation, Exception error);
    }
}
