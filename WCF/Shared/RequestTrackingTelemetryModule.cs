using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using System;
using System.Net;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Telemetry module that collects requests to WCF services
    /// </summary>
    public sealed class RequestTrackingTelemetryModule : IWcfTelemetryModule
    {
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        public RequestTrackingTelemetryModule()
        {
        }

        void ITelemetryModule.Initialize(TelemetryConfiguration configuration)
        {
            this.telemetryClient = new TelemetryClient(configuration);
            this.telemetryClient.Context.GetInternalContext().SdkVersion = "wcf: " + SdkVersionUtils.GetAssemblyVersion();
        }

        void IWcfTelemetryModule.OnBeginRequest(IOperationContext operation)
        {
            if ( operation == null )
                throw new ArgumentNullException("operation");
            if ( telemetryClient == null )
                return;

            RequestTelemetry telemetry = operation.Request;
            telemetry.Start();

            telemetry.Url = operation.EndpointUri;
            telemetry.Name = operation.OperationName;

            var httpHeaders = operation.GetHttpRequestHeaders();
            if ( httpHeaders != null )
            {
                telemetry.HttpMethod = httpHeaders.Method;
                if ( operation.ToHeader != null )
                {
                    // overwrite it for WebHttpBinding requests
                    telemetry.Url = operation.ToHeader;
                }
            }
        }

        void IWcfTelemetryModule.OnEndRequest(IOperationContext operation, Message reply)
        {
            if ( operation == null )
                throw new ArgumentNullException("operation");
            if ( telemetryClient == null )
                return;

            RequestTelemetry telemetry = operation.Request;
            telemetry.Stop();

            // make some assumptions
            bool isFault = false;
            HttpStatusCode responseCode = HttpStatusCode.OK;

            if ( reply != null )
            {
                isFault = reply.IsFault;
            }

            HttpResponseMessageProperty httpHeaders = operation.GetHttpResponseHeaders();
            if ( httpHeaders != null )
            {
                responseCode = httpHeaders.StatusCode;
                if ( (int)responseCode >= 400 )
                {
                    isFault = true;
                }
            } else if ( isFault )
            {
                responseCode = HttpStatusCode.InternalServerError;
            }

            telemetry.Success = !isFault;
            telemetry.ResponseCode = responseCode.ToString("d");
            telemetry.Properties.Add("Protocol", telemetry.Url.Scheme);
            telemetryClient.TrackRequest(telemetry);
        }

        void IWcfTelemetryModule.OnError(IOperationContext operation, Exception error)
        {
        }
    }
}
