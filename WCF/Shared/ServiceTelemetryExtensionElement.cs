using System;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Supports adding Application Insights telemetry to WCF services
    /// through the configuration file
    /// </summary>
    public class ServiceTelemetryExtensionElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        /// <summary>
        /// The Application Insights instrumentation key
        /// </summary>
        /// <remarks>
        /// You can use this as an alternative to setting the instrumentation key in the ApplicationInsights.config file
        /// </remarks>
        public String InstrumentationKey
        {
            get { return (String)base["instrumentationKey"]; }
            set { base["instrumentationKey"] = value; }
        }

        /// <summary>
        /// Gets the type of the behavior
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(ServiceTelemetryAttribute); }
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
                    properties.Add(new ConfigurationProperty("instrumentationKey", typeof(String), "", ConfigurationPropertyOptions.None));
                }
                return properties;
            }
        }

        /// <summary>
        /// Creates the ApplicationInsights behavior
        /// </summary>
        /// <returns>A new instance of <c ref="ApplicationInsightsAttribute">ApplicationInsightsAttribute</c></returns>
        protected override object CreateBehavior()
        {
            var behavior = new ServiceTelemetryAttribute();
            behavior.InstrumentationKey = InstrumentationKey;
            return behavior;
        }
    }
}
