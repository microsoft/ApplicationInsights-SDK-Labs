using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.Diagnostics.Instrumentation.Extensions.Intercept;
using System;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Provides telemetry for web service calls done through the
    /// WCF client-side stack.
    /// </summary>
    /// <remarks>
    /// Only works if the Application Insights Profiler is available.
    /// If this is not the case, you can manually add the necessary
    /// behavior through configuration to your client endpoints.
    /// See <c href="ClientTelemetryExtensionElement">ClientTelemetryExtensionElement</c>
    /// for details.
    /// </remarks>
    public sealed class WcfDependencyTrackingTelemetryModule : ITelemetryModule
    {
        private ProfilerWcfClientProcessing wcfClientProcessing = null;
        private bool initialized = false;
        private readonly object lockObject = new object();

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
        /// <summary>
        /// Gets or sets a value indicating whether to disable runtime instrumentation.
        /// </summary>
        public bool DisableRuntimeInstrumentation { get; set; }
        /// <summary>
        /// Telemetry Client based on configuration we were initialized with
        /// </summary>
        internal TelemetryClient TelemetryClient { get; private set; }

        /// <summary>
        /// Initializes this telemetry module
        /// </summary>
        /// <param name="configuration">Application Insights configuration</param>
        public void Initialize(TelemetryConfiguration configuration)
        {
            if ( configuration == null )
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if ( !initialized )
            {
                lock ( lockObject )
                {
                    if ( !initialized )
                    {
                        try
                        {
                            this.TelemetryClient = new TelemetryClient(configuration);
                            DoInitialization(configuration);
                        } catch ( Exception ex )
                        {
                            WcfEventSource.Log.InitializationFailure(ex.ToString());
                        }
                        initialized = true;
                    }
                }
            }

        }

        private void DoInitialization(TelemetryConfiguration configuration)
        {
            this.RootOperationIdHeaderName = CorrelationHeaders.HttpStandardRootIdHeader;
            this.ParentOperationIdHeaderName = CorrelationHeaders.HttpStandardParentIdHeader;
            this.SoapHeaderNamespace = CorrelationHeaders.SoapStandardNamespace;
            this.SoapParentOperationIdHeaderName = CorrelationHeaders.SoapStandardParentIdHeader;
            this.SoapRootOperationIdHeaderName = CorrelationHeaders.SoapStandardRootIdHeader;

            if ( Decorator.IsHostEnabled() )
            {
                WcfClientEventSource.Log.ClientDependencyTrackingInfo("Profiler is attached");
                WcfClientEventSource.Log.ClientDependencyTrackingInfo("Agent version: " + Decorator.GetAgentVersion());
                if ( !DisableRuntimeInstrumentation )
                {
                    this.wcfClientProcessing = new ProfilerWcfClientProcessing(this);
                    DecorateProfilerForWcfClientProcessing();
                } else
                {
                    WcfClientEventSource.Log.ClientDependencyTrackingInfo("Runtime Instrumentation is disabled.");
                }
            }
        }

        private void DecorateProfilerForWcfClientProcessing()
        {
            const String assembly = "System.ServiceModel";
            const String module = "System.ServiceModel.dll";
            const String className = "System.ServiceModel.ChannelFactory";

            // void InitializeEndpoint(ServiceEndpoint endpoint)
            Decorator.Decorate(
                assembly, module,
                className + ".InitializeEndpoint",
                1,
                null,
                this.wcfClientProcessing.OnEndInitializeEndpoint1,
                null);

            // void InitializeEndpoint(Binding binding, EndpointAddress address)
            // void InitializeEndpoint(string configurationName, EndpointAddress address)
            Decorator.Decorate(
                assembly, module,
                className + ".InitializeEndpoint",
                2,
                this.wcfClientProcessing.OnStartInitializeEndpoint2,
                this.wcfClientProcessing.OnEndInitializeEndpoint2,
                null);

            // void InitializeEndpoint(string configurationName, EndpointAddress address, Configuration configuration)
            Decorator.Decorate(
                assembly, module,
                className + ".InitializeEndpoint",
                3,
                null,
                this.wcfClientProcessing.OnEndInitializeEndpoint3,
                null);
        }
    }
}
