using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using System;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Telemetry initializer that collects the operation ID
    /// </summary>
    public sealed class OperationIdTelemetryInitializer : WcfTelemetryInitializer
    {
        private const String RequestIdProperty = "AIRequestId";

        /// <summary>
        /// Called when a telemetry item is available
        /// </summary>
        /// <param name="telemetry">The telemetry item to augment</param>
        /// <param name="operation">The operation context</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            // don't have a unique request ID to use at this point.
            String id = operation.OperationId;
            telemetry.Context.Operation.Id = id;

            RequestTelemetry request = telemetry as RequestTelemetry;
            if ( request != null )
            {
                request.Id = id;
            }
        }
    }
}
