using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Wcf.Implementation;
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
            Uri endpointUri = operation.EndpointUri;
            String path = endpointUri.AbsolutePath;

            // TODO: We probably only want to provide the method
            // if service is using WebHttpBinding.
            var httpHeaders = operation.GetHttpRequestHeaders();
            String name;
            if ( httpHeaders != null )
            {
                String method = httpHeaders.Method;
                name = String.Format("{0} {1}.{2}", method, operation.ContractName, operation.OperationName);
            } else
            {
                // Don't use the URL anymore, rather use the contract name
                name = String.Format("{0}.{1}", operation.ContractName, operation.OperationName);
            }
            telemetry.Context.Operation.Name = name;
            RequestTelemetry request = telemetry as RequestTelemetry;
            if ( request != null )
            {
                request.Name = name;
            }
        }
    }
}
