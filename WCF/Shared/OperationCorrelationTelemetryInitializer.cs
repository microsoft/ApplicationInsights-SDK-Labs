using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Wcf.Implementation;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// A telemetry initializer that will set the correlation context for all telemetry items in web application.
    /// </summary>
    public sealed class OperationCorrelationTelemetryInitializer : WcfTelemetryInitializer
    {
        /// <summary>
        /// Gets or sets the name of the HTTP header to get root operation Id from.
        /// </summary>
        public string RootOperationIdHeaderName { get; set; }
        /// <summary>
        /// Gets or sets the name of the HTTP header to get parent operation Id from.
        /// </summary>
        public string ParentOperationIdHeaderName { get; set; }
        /// <summary>
        /// Gets or sets the name of the SOAP header to get root operation Id from.
        /// </summary>
        public string SoapRootOperationIdHeaderName { get; set; }
        /// <summary>
        /// Gets or sets the name of the SOAP header to get parent operation Id from.
        /// </summary>
        public string SoapParentOperationIdHeaderName { get; set; }
        /// <summary>
        /// Gets or sets the XML Namespace for the root/parent operation ID SOAP headers.
        /// </summary>
        public string SoapHeaderNamespace { get; set; }

        private const String RequestHeadersChecked = "OCTI_RequestHeadersChecked";
        private static readonly object Checked = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="OperationCorrelationTelemetryInitializer"/> class.
        /// </summary>
        public OperationCorrelationTelemetryInitializer()
        {
            this.RootOperationIdHeaderName = CorrelationHeaders.HttpStandardRootIdHeader;
            this.ParentOperationIdHeaderName = CorrelationHeaders.HttpStandardParentIdHeader;
            this.SoapHeaderNamespace = CorrelationHeaders.SoapStandardNamespace;
            this.SoapParentOperationIdHeaderName = CorrelationHeaders.SoapStandardParentIdHeader;
            this.SoapRootOperationIdHeaderName = CorrelationHeaders.SoapStandardRootIdHeader;
        }

        /// <summary>
        /// Initialize the telemetry event
        /// </summary>
        /// <param name="telemetry">The telemetry event</param>
        /// <param name="operation">WCF operation context</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            var parentContext = operation.Request.Context.Operation;

            // if the parent operation ID is specified in the header
            // set it on the current request
            if ( String.IsNullOrEmpty(parentContext.ParentId) )
            {
                if ( !String.IsNullOrEmpty(ParentOperationIdHeaderName) )
                {
                    var parentId = GetHeader(operation, ParentOperationIdHeaderName, SoapParentOperationIdHeaderName); 
                    if ( !String.IsNullOrEmpty(parentId) )
                    {
                        parentContext.ParentId = parentId;
                    }
                }
            }
            // if the root operation ID is specified in the header
            // set it on the current request
            if ( String.IsNullOrEmpty(parentContext.Id) )
            {
                if ( !String.IsNullOrWhiteSpace(RootOperationIdHeaderName) )
                {
                    var rootId = GetHeader(operation, RootOperationIdHeaderName, SoapRootOperationIdHeaderName); 
                    if ( !String.IsNullOrEmpty(rootId) )
                    {
                        parentContext.Id = rootId;
                    }
                }
                // if the root ID has not been set, set it now
                if ( String.IsNullOrEmpty(parentContext.Id) )
                {
                    parentContext.Id = operation.Request.Id;
                }
            }
            if ( telemetry != operation.Request )
            {
                // tie the current telemetry event to the parent request
                if ( String.IsNullOrEmpty(telemetry.Context.Operation.ParentId) )
                {
                    telemetry.Context.Operation.ParentId = operation.Request.Id;
                }
                if ( String.IsNullOrEmpty(telemetry.Context.Operation.Id) )
                {
                    telemetry.Context.Operation.Id = parentContext.Id;
                }
            } else
            {
                // we've initialized the correlation headers for
                // this request object. We want to initialize
                // all other telemetry events from this request
                // without checking the WCF message headers
                // ever again, as this can trigger ObjectDisposedException
                // errors when TelemetryClient.Initialize() is called
                // at the end of the request
                ((IOperationContextState)operation).SetState(RequestHeadersChecked, Checked);
            }
        }

        private String GetHeader(IOperationContext context, String httpHeader, String soapHeader)
        {
            if ( RequestAlreadyChecked(context) )
            {
                return null;
            }
            var httpHeaders = context.GetHttpRequestHeaders();
            if ( httpHeaders != null )
            {
                return httpHeaders.Headers[httpHeader];
            } else
            {
                return context.GetIncomingMessageHeader<String>(soapHeader, SoapHeaderNamespace);
            }
        }

        private bool RequestAlreadyChecked(IOperationContext context)
        {
            object checkedValue = null;
            if ( ((IOperationContextState)context).TryGetState(RequestHeadersChecked, out checkedValue) )
            {
                return checkedValue == Checked;
            }
            return false;
        }
    }
}
