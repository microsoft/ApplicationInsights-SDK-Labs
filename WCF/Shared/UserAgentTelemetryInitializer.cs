using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using System;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Telemetry initializer that collects User-Agent information
    /// </summary>
    public sealed class UserAgentTelemetryInitializer : WcfTelemetryInitializer
    {
        private const String UserAgent = "UATI_UserAgent";

        /// <summary>
        /// Called when a telemetry item is available
        /// </summary>
        /// <param name="telemetry">The telemetry item to augment</param>
        /// <param name="operation">The operation context</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            if ( String.IsNullOrEmpty(telemetry.Context.User.UserAgent) )
            {
                var requestContext = operation.Request.Context.User;
                if ( String.IsNullOrEmpty(requestContext.UserAgent) )
                {
                    UpdateUserAgent(operation, requestContext);
                }
                var userContext = telemetry.Context.User;
                telemetry.Context.User.UserAgent = requestContext.UserAgent;
            }
        }

        private void UpdateUserAgent(IOperationContext operation, UserContext userContext)
        {
            var contextState = (IOperationContextState)operation;
            String knownAgent = null;
            if ( contextState.TryGetState(UserAgent, out knownAgent) )
            {
                userContext.Id = knownAgent;
                return;
            }

            var httpHeaders = operation.GetHttpRequestHeaders();
            if ( httpHeaders != null )
            {
                var userAgent = httpHeaders.Headers["User-Agent"];
                if ( !String.IsNullOrEmpty(userAgent) )
                {
                    userContext.UserAgent = userAgent;
                }
            }
            // we store this here (even if it's null), to avoid
            // having to check the message headers later on
            // when it might no longer be available.
            contextState.SetState(UserAgent, userContext.UserAgent ?? "");
        }
    }
}
