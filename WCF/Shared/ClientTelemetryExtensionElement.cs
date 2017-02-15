using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Supports generating Application Insights dependency
    /// events for calls to Web Services done using the
    /// WCF client-side stack through the configuration file
    /// </summary>
    public sealed class ClientTelemetryExtensionElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        /// <summary>
        /// Gets the type of the behavior
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(ClientTelemetryEndpointBehavior); }
        }

        /// <summary>
        /// Gets or sets the name of the HTTP header to get root operation Id from.
        /// </summary>
        public String RootOperationIdHeaderName
        {
            get { return (String)base["rootOperationIdHeaderName"]; }
            set { base["rootOperationIdHeaderName"] = value; }
        }
        /// <summary>
        /// Gets or sets the name of the HTTP header to get parent operation Id from.
        /// </summary>
        public String ParentOperationIdHeaderName
        {
            get { return (String)base["parentOperationIdHeaderName"]; }
            set { base["parentOperationIdHeaderName"] = value; }
        }
        /// <summary>
        /// Gets or sets the name of the SOAP header to get root operation Id from.
        /// </summary>
        public String SoapRootOperationIdHeaderName
        {
            get { return (String)base["soapRootOperationIdHeaderName"]; }
            set { base["soapRootOperationIdHeaderName"] = value; }
        }
        /// <summary>
        /// Gets or sets the name of the SOAP header to get parent operation Id from.
        /// </summary>
        public String SoapParentOperationIdHeaderName
        {
            get { return (String)base["soapParentOperationIdHeaderName"]; }
            set { base["soapParentOperationIdHeaderName"] = value; }
        }
        /// <summary>
        /// Gets or sets the XML Namespace for the root/parent operation ID SOAP headers.
        /// </summary>
        public String SoapHeaderNamespace
        {
            get { return (String)base["soapHeaderNamespace"]; }
            set { base["soapHeaderNamespace"] = value; }
        }

        /// <summary>
        /// The list of properties supported by this behavior
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if ( properties == null )
                {
                    properties = CreateProperties();
                }
                return properties;
            }
        }

        /// <summary>
        /// Creates the ApplicationInsights behavior
        /// </summary>
        /// <returns>A new Endpoint Behavior that will track client-side calls</returns>
        protected override object CreateBehavior()
        {
            return CreateBehaviorInternal();
        }

        internal ClientTelemetryEndpointBehavior CreateBehaviorInternal()
        {
            var behavior = new ClientTelemetryEndpointBehavior(TelemetryConfiguration.Active);
            behavior.ParentOperationIdHeaderName = ParentOperationIdHeaderName;
            behavior.RootOperationIdHeaderName = RootOperationIdHeaderName;
            behavior.SoapParentOperationIdHeaderName = SoapParentOperationIdHeaderName;
            behavior.SoapRootOperationIdHeaderName = SoapRootOperationIdHeaderName;
            behavior.SoapHeaderNamespace = SoapHeaderNamespace;
            return behavior;
        }
        internal ConfigurationPropertyCollection CreateProperties()
        {
            var props = new ConfigurationPropertyCollection();
            props.Add(new ConfigurationProperty("parentOperationIdHeaderName", typeof(String), CorrelationHeaders.HttpStandardParentIdHeader));
            props.Add(new ConfigurationProperty("rootOperationIdHeaderName", typeof(String), CorrelationHeaders.HttpStandardRootIdHeader));
            props.Add(new ConfigurationProperty("soapParentOperationIdHeaderName", typeof(String), CorrelationHeaders.SoapStandardParentIdHeader));
            props.Add(new ConfigurationProperty("soapRootOperationIdHeaderName", typeof(String), CorrelationHeaders.SoapStandardRootIdHeader));
            props.Add(new ConfigurationProperty("soapHeaderNamespace", typeof(String), CorrelationHeaders.SoapStandardNamespace));
            return props;
        }
    }
}
