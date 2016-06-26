using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using System;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Telemetry initializer that collects User-Agent information
    /// </summary>
    public sealed class UserAgentTelemetryInitializer : WcfTelemetryInitializer
    {
        /// <summary>
        /// Called when a telemetry item is available
        /// </summary>
        /// <param name="telemetry">The telemetry item to augment</param>
        /// <param name="operation">The operation context</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            var httpHeaders = operation.GetHttpRequestHeaders();
            if ( httpHeaders != null )
            {
                var userAgent = httpHeaders.Headers["User-Agent"];
                if ( !String.IsNullOrEmpty(userAgent) )
                {
                    telemetry.Context.User.UserAgent = userAgent;
                }
            }
        }
    }
}
