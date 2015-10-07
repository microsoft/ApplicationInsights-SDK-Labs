
namespace Microsoft.ApplicationInsights.ExceptionTracking
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Diagnostics.Instrumentation.Extensions.Intercept;

    public class ExceptionTrackingTelemetryModule : ITelemetryModule
    {
        private TelemetryClient telemetryClient;

        private ICollection<Function> exceptionTracking = new List<Function>();

        public ICollection<Function> ExceptionTracking
        {
            get
            {
                return this.exceptionTracking;
            }
        }

        public ExceptionTrackingTelemetryModule()
        {
            // do nothing
        }

        public void Initialize(TelemetryConfiguration configuration)
        {
            this.telemetryClient = new TelemetryClient(configuration);

            foreach (var func in this.ExceptionTracking)
            {
                Decorator.Decorate(
                    func.AssemblyName, 
                    func.ModuleName, 
                    func.Name, 
                    func.ArgumentsCount, 
                    OnBeginForGetResponse, 
                    OnEndForGetResponse, 
                    OnExceptionForGetResponse);
            }
        }

        public object OnBeginForGetResponse(object thisObj)
        {
            return null;
        }

        public object OnEndForGetResponse(object context, object returnValue, object thisObj)
        {
            return returnValue;
        }

        public void OnExceptionForGetResponse(object context, object exception, object thisObj)
        {
            var excTelemetry = new ExceptionTelemetry((Exception)exception);
            excTelemetry.Properties["this"] = thisObj.ToString();
            this.telemetryClient.TrackException(excTelemetry);
        }
    }
}