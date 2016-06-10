using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using System;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

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
            if ( String.IsNullOrEmpty(telemetry.Context.User.UserAgent) )
            {
                var userContext = telemetry.Context.User;
                if ( String.IsNullOrEmpty(userContext.UserAgent) )
                {
                    UpdateUserAgent(operation, userContext);
                }
                telemetry.Context.User.UserAgent = userContext.UserAgent;
            }
        }

        private void UpdateUserAgent(IOperationContext operation, UserContext userContext)
        {
            var httpHeaders = operation.GetHttpRequestHeaders();
            if ( httpHeaders != null )
            {
                var userAgent = httpHeaders.Headers["User-Agent"];
                if ( !String.IsNullOrEmpty(userAgent) )
                {
                    userContext.UserAgent = userAgent;
                }
            }
        }
    }
}
