namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Configuration;
    using Microsoft.ApplicationInsights.Extensibility;

    /// <summary>
    /// Supports generating Application Insights dependency
    /// events for calls to Web Services done using the
    /// WCF client-side stack through the configuration file.
    /// </summary>
    public sealed class ClientTelemetryExtensionElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        /// <summary>
        /// Gets the type of the behavior.
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(ClientTelemetryEndpointBehavior); }
        }

        /// <summary>
        /// Gets or sets the name of the HTTP header to get root operation Id from.
        /// </summary>
        public string RootOperationIdHeaderName
        {
            get { return (string)base["rootOperationIdHeaderName"]; }
            set { base["rootOperationIdHeaderName"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the HTTP header to get parent operation Id from.
        /// </summary>
        public string ParentOperationIdHeaderName
        {
            get { return (string)base["parentOperationIdHeaderName"]; }
            set { base["parentOperationIdHeaderName"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the SOAP header to get root operation Id from.
        /// </summary>
        public string SoapRootOperationIdHeaderName
        {
            get { return (string)base["soapRootOperationIdHeaderName"]; }
            set { base["soapRootOperationIdHeaderName"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the SOAP header to get parent operation Id from.
        /// </summary>
        public string SoapParentOperationIdHeaderName
        {
            get { return (string)base["soapParentOperationIdHeaderName"]; }
            set { base["soapParentOperationIdHeaderName"] = value; }
        }

        /// <summary>
        /// Gets or sets the XML Namespace for the root/parent operation ID SOAP headers.
        /// </summary>
        public string SoapHeaderNamespace
        {
            get { return (string)base["soapHeaderNamespace"]; }
            set { base["soapHeaderNamespace"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether channel events (such as channel open) should be emitted as dependencies.
        /// </summary>
        public bool IgnoreChannelEvents
        {
            get { return (bool)base["ignoreChannelEvents"]; }
            set { base["ignoreChannelEvents"] = value; }
        }

        /// <summary>
        /// The list of properties supported by this behavior.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = this.CreateProperties();
                }

                return this.properties;
            }
        }

        internal ClientTelemetryEndpointBehavior CreateBehaviorInternal()
        {
            var behavior = new ClientTelemetryEndpointBehavior(TelemetryConfiguration.Active)
            {
                ParentOperationIdHeaderName = this.ParentOperationIdHeaderName,
                RootOperationIdHeaderName = this.RootOperationIdHeaderName,
                SoapParentOperationIdHeaderName = this.SoapParentOperationIdHeaderName,
                SoapRootOperationIdHeaderName = this.SoapRootOperationIdHeaderName,
                SoapHeaderNamespace = this.SoapHeaderNamespace,
                IgnoreChannelEvents = this.IgnoreChannelEvents
            };
            return behavior;
        }

        internal ConfigurationPropertyCollection CreateProperties()
        {
            var props = new ConfigurationPropertyCollection
            {
                new ConfigurationProperty("parentOperationIdHeaderName", typeof(string), CorrelationHeaders.HttpStandardParentIdHeader),
                new ConfigurationProperty("rootOperationIdHeaderName", typeof(string), CorrelationHeaders.HttpStandardRootIdHeader),
                new ConfigurationProperty("soapParentOperationIdHeaderName", typeof(string), CorrelationHeaders.SoapStandardParentIdHeader),
                new ConfigurationProperty("soapRootOperationIdHeaderName", typeof(string), CorrelationHeaders.SoapStandardRootIdHeader),
                new ConfigurationProperty("soapHeaderNamespace", typeof(string), CorrelationHeaders.SoapStandardNamespace),
                new ConfigurationProperty("ignoreChannelEvents", typeof(bool), false)
            };
            return props;
        }

        /// <summary>
        /// Creates the ApplicationInsights behavior.
        /// </summary>
        /// <returns>A new Endpoint Behavior that will track client-side calls.</returns>
        protected override object CreateBehavior()
        {
            return this.CreateBehaviorInternal();
        }
    }
}
