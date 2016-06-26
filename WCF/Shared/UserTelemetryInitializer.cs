using Microsoft.ApplicationInsights.Channel;
using System;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Telemetry initializer that collects user information from the security context
    /// </summary>
    public sealed class UserTelemetryInitializer : WcfTelemetryInitializer
    {
        /// <summary>
        /// Called when a telemetry item is available
        /// </summary>
        /// <param name="telemetry">The telemetry item to augment</param>
        /// <param name="operation">The operation context</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            var ctxt = operation.SecurityContext;
            if ( ctxt == null || ctxt.IsAnonymous )
                return;

            if ( ctxt.PrimaryIdentity != null )
            {
                telemetry.Context.User.Id = ctxt.PrimaryIdentity.Name;
            }
        }
    }
}
