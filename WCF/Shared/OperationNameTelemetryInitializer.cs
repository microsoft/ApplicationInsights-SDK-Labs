using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using System;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Telemetry initializer that collects the operation name
    /// </summary>
    public sealed class OperationNameTelemetryInitializer : WcfTelemetryInitializer
    {
        /// <summary>
        /// Called when a telemetry item is available
        /// </summary>
        /// <param name="telemetry">The telemetry item to augment</param>
        /// <param name="operation">The operation context</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            // TODO: consider including the HTTP verb
            // if service is using WebHttpBinding.
            String name = operation.ContractName + '.' + operation.OperationName;

            telemetry.Context.Operation.Name = name;
            RequestTelemetry request = telemetry as RequestTelemetry;
            if ( request != null )
            {
                request.Name = name;
            }
        }
    }
}
