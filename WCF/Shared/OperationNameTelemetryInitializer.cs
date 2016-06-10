using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

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
            if ( String.IsNullOrEmpty(telemetry.Context.Operation.Name) )
            {
                var opContext = operation.Request.Context.Operation;
                if ( String.IsNullOrEmpty(opContext.Name) )
                {
                    UpdateOperationContext(operation, opContext);
                }
                telemetry.Context.Operation.Name = opContext.Name;

                RequestTelemetry request = telemetry as RequestTelemetry;
                if ( request != null )
                {
                    request.Name = opContext.Name;
                }
            }
        }

        private void UpdateOperationContext(IOperationContext operation, OperationContext opContext)
        {
            opContext.Name = operation.ContractName + '.' + operation.OperationName;
        }
    }
}
