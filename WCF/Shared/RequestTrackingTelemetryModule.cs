namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using System.Net;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Implementation;

    /// <summary>
    /// Telemetry module that collects requests to WCF services.
    /// </summary>
    public sealed class RequestTrackingTelemetryModule : IWcfTelemetryModule
    {
        private TelemetryClient telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestTrackingTelemetryModule"/> class.
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
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            if (this.telemetryClient == null)
            {
                return;
            }

            RequestTelemetry telemetry = operation.Request;

            // if ASP.NET has already started the request, leave the start time alone.
            if (operation.OwnsRequest)
            {
                telemetry.Start();
            }

            telemetry.Url = operation.EndpointUri;
            telemetry.Name = operation.OperationName;
            telemetry.Properties["soapAction"] = operation.SoapAction;

            var httpHeaders = operation.GetHttpRequestHeaders();
            if (httpHeaders != null)
            {
                telemetry.Properties["httpMethod"] = httpHeaders.Method;
                if (operation.ToHeader != null)
                {
                    // overwrite it for WebHttpBinding requests
                    telemetry.Url = operation.ToHeader;
                }
            }

            // run telemetry initializers here, while the request message is still open
            this.telemetryClient.Initialize(telemetry);
        }

        void IWcfTelemetryModule.OnEndRequest(IOperationContext operation, Message reply)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }

            if (this.telemetryClient == null)
            {
                return;
            }

            if (reply != null && reply.IsClosed())
            {
                WcfEventSource.Log.ResponseMessageClosed(nameof(RequestTrackingTelemetryModule), "OnEndRequest");
            }

            RequestTelemetry telemetry = operation.Request;

            // make some assumptions
            var isFault = false;
            var responseCode = HttpStatusCode.OK;

            if (reply != null && !reply.IsClosed())
            {
                isFault = reply.IsFault;
            }

            HttpResponseMessageProperty httpHeaders = operation.GetHttpResponseHeaders();
            if (httpHeaders != null)
            {
                responseCode = httpHeaders.StatusCode;
                if ((int)responseCode >= 400)
                {
                    isFault = true;
                }
            }
            else if (isFault)
            {
                responseCode = HttpStatusCode.InternalServerError;
            }

            // if the operation code has already marked the request as failed
            // don't overwrite the value if we think it was successful
            if (isFault || !telemetry.Success.HasValue)
            {
                telemetry.Success = !isFault;
            }

            telemetry.ResponseCode = responseCode.ToString("d");
            if (telemetry.Url != null)
            {
                telemetry.Properties["protocol"] = telemetry.Url.Scheme;
            }

            // if the Microsoft.ApplicationInsights.Web package started
            // tracking this request before WCF handled it, we
            // don't want to track it because it would duplicate the event.
            // Let the HttpModule instead write it later on.
            if (operation.OwnsRequest)
            {
                telemetry.Stop();
                this.telemetryClient.TrackRequest(telemetry);
            }
        }

        void IWcfTelemetryModule.OnError(IOperationContext operation, Exception error)
        {
        }
    }
}
