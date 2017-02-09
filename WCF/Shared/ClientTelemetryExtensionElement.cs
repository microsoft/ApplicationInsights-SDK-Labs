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
    public class ClientTelemetryExtensionElement : BehaviorExtensionElement
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
        /// The list of properties supported by this behavior
        /// </summary>
        protected override System.Configuration.ConfigurationPropertyCollection Properties
        {
            get
            {
                if ( properties == null )
                {
                    properties = new ConfigurationPropertyCollection();
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
            return new ClientTelemetryEndpointBehavior(TelemetryConfiguration.Active);
        }
    }
}
