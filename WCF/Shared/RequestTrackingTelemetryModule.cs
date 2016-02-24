using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
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
        }

        void IWcfTelemetryModule.OnBeginRequest(IOperationContext operation, IRequestHolder svcreq)
        {
            if ( operation == null )
                throw new ArgumentNullException("operation");
            if ( svcreq == null )
                throw new ArgumentNullException("svcreq");
            if ( telemetryClient == null )
                return;

            svcreq.Start();

            RequestTelemetry telemetry = svcreq.Request;
            telemetry.StartTime = svcreq.StartedAt;
            telemetry.Url = operation.EndpointUri;
            telemetry.Name = operation.OperationName;

            var httpHeaders = operation.GetHttpRequestHeaders();
            if ( httpHeaders != null )
            {
                telemetry.HttpMethod = httpHeaders.Method;
            }
        }

        void IWcfTelemetryModule.OnEndRequest(IOperationContext operation, IRequestHolder svcreq, Message reply)
        {
            if ( operation == null )
                throw new ArgumentNullException("operation");
            if ( svcreq == null )
                throw new ArgumentNullException("svcreq");
            if ( telemetryClient == null )
                return;

            RequestTelemetry telemetry = svcreq.Request;
            telemetry.Duration = svcreq.Stop();

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
