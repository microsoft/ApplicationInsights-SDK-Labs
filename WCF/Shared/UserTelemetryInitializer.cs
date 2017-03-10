using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Telemetry initializer that collects user information from the security context
    /// </summary>
    public sealed class UserTelemetryInitializer : WcfTelemetryInitializer
    {
        private const String IdentityName = "UTI_IdentityName";

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
            var contextState = (IOperationContextState)operation;
            String knownIdentity = null;
            if ( contextState.TryGetState(IdentityName, out knownIdentity) )
            {
                userContext.Id = knownIdentity;
                return;
            }

            var ctxt = operation.SecurityContext;
            if ( ctxt != null && !ctxt.IsAnonymous && ctxt.PrimaryIdentity != null )
            {
                userContext.Id = ctxt.PrimaryIdentity.Name;
            }

            // we store this here (even if it's null), to avoid
            // having to check the request security context later on
            // when it might no longer be available.
            contextState.SetState(IdentityName, userContext.Id ?? "");
        }
    }
}
