using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System.ServiceModel.Channels;
using Microsoft.ApplicationInsights.Wcf.Implementation;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Collects the IP of the client calling the WCF service
    /// </summary>
    public sealed class ClientIpTelemetryInitializer : WcfTelemetryInitializer
    {
        /// <summary>
        /// Initialize the telemetry event with the client IP if available
        /// </summary>
        /// <param name="telemetry">The telemetry event</param>
        /// <param name="operation">The WCF operation context</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            if ( String.IsNullOrEmpty(telemetry.Context.Location.Ip) )
            {
                var location = operation.Request.Context.Location;
                if ( String.IsNullOrEmpty(location.Ip) )
                {
                    UpdateClientIp(location, operation);
                }
                telemetry.Context.Location.Ip = location.Ip;
            }
        }

        private void UpdateClientIp(LocationContext location, IOperationContext operation)
        {
            if ( operation.HasIncomingMessageProperty(RemoteEndpointMessageProperty.Name) )
            {
                var property = (RemoteEndpointMessageProperty)
                    operation.GetIncomingMessageProperty(RemoteEndpointMessageProperty.Name);
                location.Ip = property.Address;
                WcfEventSource.Log.LocationIdSet(location.Ip);
            }
        }
    }
}
