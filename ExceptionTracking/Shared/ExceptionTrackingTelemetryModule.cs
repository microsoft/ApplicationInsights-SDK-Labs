
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

            var extesionBaseDirectory = string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.RelativeSearchPath)
                ? AppDomain.CurrentDomain.BaseDirectory
                : AppDomain.CurrentDomain.RelativeSearchPath;

            Decorator.InitializeExtension(extesionBaseDirectory);


            foreach (var func in this.ExceptionTracking)
            {
                Decorator.Decorate(
                    func.AssemblyName, 
                    func.ModuleName, 
                    func.Name, 
                    func.ArgumentsCount, 
                    OnBeginEmpty, 
                    OnEndEmpty, 
                    OnExceptionTrackException);
            }
        }

        public object OnBeginEmpty(object thisObj)
        {
            Console.Out.WriteLine("OnBeginEmpty");
            return null;
        }

        public object OnEndEmpty(object context, object returnValue, object thisObj)
        {
            Console.Out.WriteLine("OnEndEmpty");
            return returnValue;
        }

        public void OnExceptionTrackException(object context, object exception, object thisObj)
        {
            Console.Out.WriteLine("OnExceptionTrackException");
            var excTelemetry = new ExceptionTelemetry((Exception)exception);
            excTelemetry.Properties["this"] = thisObj.ToString();
            this.telemetryClient.TrackException(excTelemetry);
        }
    }
}