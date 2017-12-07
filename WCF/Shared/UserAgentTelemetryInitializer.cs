namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Implementation;

    /// <summary>
    /// Telemetry initializer that collects User-Agent information.
    /// </summary>
    public sealed class UserAgentTelemetryInitializer : WcfTelemetryInitializer
    {
        private const string UserAgent = "UATI_UserAgent";

        /// <summary>
        /// Called when a telemetry item is available.
        /// </summary>
        /// <param name="telemetry">The telemetry item to augment.</param>
        /// <param name="operation">The operation context.</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            if (string.IsNullOrEmpty(telemetry.Context.User.UserAgent))
            {
                var requestContext = operation.Request.Context.User;
                if (string.IsNullOrEmpty(requestContext.UserAgent))
                {
                    this.UpdateUserAgent(operation, requestContext);
                }

                var userContext = telemetry.Context.User;
                telemetry.Context.User.UserAgent = requestContext.UserAgent;
            }
        }

        private void UpdateUserAgent(IOperationContext operation, UserContext userContext)
        {
            var contextState = (IOperationContextState)operation;
            string knownAgent = null;
            if (contextState.TryGetState(UserAgent, out knownAgent))
            {
                userContext.UserAgent = knownAgent;
                return;
            }

            var httpHeaders = operation.GetHttpRequestHeaders();
            if (httpHeaders != null)
            {
                var userAgent = httpHeaders.Headers["User-Agent"];
                if (!string.IsNullOrEmpty(userAgent))
                {
                    userContext.UserAgent = userAgent;
                }
            }

            // we store this here (even if it's null), to avoid
            // having to check the message headers later on
            // when it might no longer be available.
            contextState.SetState(UserAgent, userContext.UserAgent ?? string.Empty);
        }
    }
}
