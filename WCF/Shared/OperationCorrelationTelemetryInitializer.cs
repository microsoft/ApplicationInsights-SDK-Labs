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
        private const string StandardParentIdHeader = "x-ms-request-id";
        private const string StandardRootIdHeader = "x-ms-request-root-id";        /// <summary>

        /// <summary>
        /// Gets or sets the name of the header to get root operation Id from.
        /// </summary>
        public string RootOperationIdHeaderName { get; set; }
        /// <summary>
        /// Gets or sets the name of the header to get parent operation Id from.
        /// </summary>
        public string ParentOperationIdHeaderName { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="OperationCorrelationTelemetryInitializer"/> class.
        /// </summary>
        public OperationCorrelationTelemetryInitializer()
        {
            this.RootOperationIdHeaderName = StandardRootIdHeader;
            this.ParentOperationIdHeaderName = StandardParentIdHeader;
        }

        /// <summary>
        /// Initialize the telemetry event
        /// </summary>
        /// <param name="telemetry">The telemetry event</param>
        /// <param name="operation">WCF operation context</param>
        protected override void OnInitialize(ITelemetry telemetry, IOperationContext operation)
        {
            // TODO: Enable passing value through
            // non -HTTP headers (such as message header)
            var parentContext = operation.Request.Context.Operation;

            // if the parent operation ID is specified in the header
            // set it on the current request
            if ( String.IsNullOrEmpty(parentContext.ParentId) )
            {
                if ( !String.IsNullOrEmpty(ParentOperationIdHeaderName) )
                {
                    var parentId = GetHeader(operation, ParentOperationIdHeaderName); 
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
                    var rootId = GetHeader(operation, RootOperationIdHeaderName); 
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
            }
        }

        private String GetHeader(IOperationContext context, String name)
        {
            var httpHeaders = context.GetHttpRequestHeaders();
            if ( httpHeaders != null )
            {
                return httpHeaders.Headers[name];
            }
            return null;
        }
    }
}
