namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Wcf.Implementation;

    /// <summary>
    /// Base class for ITelemetryInitializer implementations used
    /// to instrument WCF services.
    /// </summary>
    public abstract class WcfTelemetryInitializer : ITelemetryInitializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WcfTelemetryInitializer"/> class.
        /// </summary>
        public WcfTelemetryInitializer()
        {
            WcfEventSource.Log.WcfTelemetryInitializerLoaded(GetType().FullName);
        }

        /// <summary>
        /// Initializes the telemetry context for a telemetry item.
        /// </summary>
        /// <param name="telemetry">The telemetry item to augment.</param>
        public void Initialize(ITelemetry telemetry)
        {
            var context = WcfOperationContext.Current;
            if (context != null)
            {
                this.OnInitialize(telemetry, context);
            }
            else
            {
                WcfEventSource.Log.NoOperationContextFound();
            }
        }

        /// <summary>
        /// Initializes the telemetry context for a telemetry item.
        /// </summary>
        /// <param name="telemetry">The telemetry item to augment.</param>
        /// <param name="context">The operation context.</param>
        /// <remarks>
        /// This variant is used to support easier testability by providing
        /// the operation context explicitly.
        /// </remarks>
        public void Initialize(ITelemetry telemetry, IOperationContext context)
        {
            this.OnInitialize(telemetry, context);
        }

        /// <summary>
        /// Method that subclasses can override to augment
        /// a telemetry item.
        /// </summary>
        /// <param name="telemetry">The telemetry item.</param>
        /// <param name="operation">The operation context.</param>
        protected abstract void OnInitialize(ITelemetry telemetry, IOperationContext operation);
    }
}
