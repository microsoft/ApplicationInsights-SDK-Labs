using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Implements exception tracking for WCF services
    /// </summary>
    public sealed class ExceptionTrackingTelemetryModule : IWcfTelemetryModule
    {
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of this module
        /// </summary>
        public ExceptionTrackingTelemetryModule()
        {
        }

        void ITelemetryModule.Initialize(TelemetryConfiguration configuration)
        {
            this.telemetryClient = new TelemetryClient(configuration);
        }

        void IWcfTelemetryModule.OnBeginRequest(IOperationContext operation)
        {
        }

        void IWcfTelemetryModule.OnEndRequest(IOperationContext operation, Message reply)
        {
        }

        void IWcfTelemetryModule.OnError(IOperationContext operation, Exception error)
        {
            if ( operation == null )
                throw new ArgumentNullException("operation");
            if ( error == null )
                throw new ArgumentNullException("error");
            if ( telemetryClient == null )
                return;

            ExceptionTelemetry telemetry = new ExceptionTelemetry(error);
            if ( error is FaultException )
            {
                telemetry.SeverityLevel = SeverityLevel.Error;
                telemetry.HandledAt = ExceptionHandledAt.UserCode;
            } else
            {
                telemetry.SeverityLevel = SeverityLevel.Critical;
                telemetry.HandledAt = ExceptionHandledAt.Platform;
            }
            telemetryClient.TrackException(error);
        }
    }
}
