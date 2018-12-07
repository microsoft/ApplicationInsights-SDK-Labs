namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Configuration;

    /// <summary>
    /// Supports adding Application Insights telemetry to WCF services
    /// through the configuration file.
    /// </summary>
    public class ServiceTelemetryExtensionElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;

        /// <summary>
        /// Gets or sets the Application Insights instrumentation key.
        /// </summary>
        /// <remarks>
        /// You can use this as an alternative to setting the instrumentation key in the ApplicationInsights.config file.
        /// </remarks>
        public string InstrumentationKey
        {
            get { return (string)base["instrumentationKey"]; }
            set { base["instrumentationKey"] = value; }
        }

        /// <summary>
        /// Gets the type of the behavior.
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(ServiceTelemetryAttribute); }
        }

        /// <summary>
        /// Gets the list of properties supported by this behavior.
        /// </summary>
        protected override System.Configuration.ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new ConfigurationPropertyCollection
                    {
                        new ConfigurationProperty("instrumentationKey", typeof(string), string.Empty, ConfigurationPropertyOptions.None)
                    };
                }

                return this.properties;
            }
        }

        /// <summary>
        /// Creates the behavior.
        /// </summary>
        /// <returns>A new instance of <see cref="ServiceTelemetryAttribute"/> class.</returns>.
        protected override object CreateBehavior()
        {
            var behavior = new ServiceTelemetryAttribute()
            {
                InstrumentationKey = this.InstrumentationKey
            };
            return behavior;
        }
    }
}
