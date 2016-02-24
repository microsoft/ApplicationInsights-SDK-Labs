using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation;

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
            throw new NotImplementedException();
        }
    }
}
