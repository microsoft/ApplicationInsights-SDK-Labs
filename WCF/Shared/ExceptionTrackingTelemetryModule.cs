namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Implementation;

    /// <summary>
    /// Implements exception tracking for WCF services.
    /// </summary>
    public sealed class ExceptionTrackingTelemetryModule : IWcfTelemetryModule
    {
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionTrackingTelemetryModule"/> class. 
        /// </summary>
        public ExceptionTrackingTelemetryModule()
        {
        }

        void ITelemetryModule.Initialize(TelemetryConfiguration configuration)
        {
            this.telemetryClient = new TelemetryClient(configuration);
            this.telemetryClient.Context.GetInternalContext().SdkVersion = "wcf: " + SdkVersionUtils.GetAssemblyVersion();
        }

        void IWcfTelemetryModule.OnBeginRequest(IOperationContext operation)
        {
        }

        void IWcfTelemetryModule.OnEndRequest(IOperationContext operation, Message reply)
        {
        }

        void IWcfTelemetryModule.OnError(IOperationContext operation, Exception error)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            if (error == null)
            {
                throw new ArgumentNullException("error");
            }

            if (this.telemetryClient == null)
            {
                return;
            }

            ExceptionTelemetry telemetry = new ExceptionTelemetry(error);
            if (error is FaultException)
            {
                telemetry.SeverityLevel = SeverityLevel.Error;
            }
            else
            {
                telemetry.SeverityLevel = SeverityLevel.Critical;
            }

            this.telemetryClient.TrackException(error);
        }
    }
}
