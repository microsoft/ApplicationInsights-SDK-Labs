namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;

    /// <summary>
    /// Telemetry initializer that collects the operation name.
    /// </summary>
    public sealed class OperationNameTelemetryInitializer : WcfTelemetryInitializer
    {
        /// <summary>
        /// Called when a telemetry item is available.
        /// </summary>
        /// <param name="telemetry">The telemetry item to augment.</param>
        /// <param name="operation">The operation context.</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Operation.Name))
            {
                var operationContext = operation.Request.Context.Operation;
                if (string.IsNullOrEmpty(operationContext.Name))
                {
                    this.UpdateOperationContext(operation, operationContext);
                }

                telemetry.Context.Operation.Name = operationContext.Name;

                var request = telemetry as RequestTelemetry;
                if (request != null)
                {
                    request.Name = operationContext.Name;
                }
            }
        }

        private void UpdateOperationContext(IOperationContext operation, OperationContext context)
        {
            context.Name = operation.ContractName + '.' + operation.OperationName;
        }
    }
}
