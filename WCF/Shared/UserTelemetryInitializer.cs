using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

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
            if ( String.IsNullOrEmpty(telemetry.Context.User.Id) )
            {
                var userContext = operation.Request.Context.User;
                if ( String.IsNullOrEmpty(userContext.Id) )
                {
                    UpdateUserContext(operation, userContext);
                }
                telemetry.Context.User.Id = userContext.Id;
            }
        }

        private void UpdateUserContext(IOperationContext operation, UserContext userContext)
        {
            var ctxt = operation.SecurityContext;
            if ( ctxt == null || ctxt.IsAnonymous )
            {
                return;
            }

            if ( ctxt.PrimaryIdentity != null )
            {
                userContext.Id = ctxt.PrimaryIdentity.Name;
            }
        }
    }
}
