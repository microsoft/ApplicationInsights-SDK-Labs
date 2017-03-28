namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Implementation;

    /// <summary>
    /// Collects the IP of the client calling the WCF service.
    /// </summary>
    public sealed class ClientIpTelemetryInitializer : WcfTelemetryInitializer
    {
        /// <summary>
        /// Initialize the telemetry event with the client IP if available.
        /// </summary>
        /// <param name="telemetry">The telemetry event.</param>
        /// <param name="operation">The WCF operation context.</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            if (string.IsNullOrEmpty(telemetry.Context.Location.Ip))
            {
                var location = operation.Request.Context.Location;
                if (string.IsNullOrEmpty(location.Ip))
                {
                    this.UpdateClientIp(location, operation);
                }

                telemetry.Context.Location.Ip = location.Ip;
            }
        }

        private void UpdateClientIp(LocationContext location, IOperationContext operation)
        {
            if (operation.HasIncomingMessageProperty(RemoteEndpointMessageProperty.Name))
            {
                var property = (RemoteEndpointMessageProperty)
                    operation.GetIncomingMessageProperty(RemoteEndpointMessageProperty.Name);
                location.Ip = property.Address;
                WcfEventSource.Log.LocationIdSet(location.Ip);
            }
        }
    }
}
