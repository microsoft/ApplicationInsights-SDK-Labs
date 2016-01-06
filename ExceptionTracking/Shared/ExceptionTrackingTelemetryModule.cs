
namespace Microsoft.ApplicationInsights.ExceptionTracking
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Diagnostics.Instrumentation.Extensions.Intercept;
    using System.Diagnostics;

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
            try
            {
                this.telemetryClient = new TelemetryClient(configuration);

                var extesionBaseDirectory = string.IsNullOrWhiteSpace(AppDomain.CurrentDomain.RelativeSearchPath)
                    ? AppDomain.CurrentDomain.BaseDirectory
                    : AppDomain.CurrentDomain.RelativeSearchPath;

                Decorator.InitializeExtension(extesionBaseDirectory);


                foreach (var func in this.ExceptionTracking)
                {
                    if (func.ArgumentsCount == 0)
                    {
                        Decorator.Decorate(
                            func.AssemblyName,
                            func.ModuleName,
                            func.Name,
                            func.ArgumentsCount,
                            null,
                            null,
                            OnExceptionTrackException);
                    }
                    else if (func.ArgumentsCount == 1)
                    {
                        Decorator.Decorate(
                            func.AssemblyName,
                            func.ModuleName,
                            func.Name,
                            func.ArgumentsCount,
                            null,
                            null,
                            OnExceptionEmpty2);
                    }
                    else if (func.ArgumentsCount == 2)
                    {
                        Decorator.Decorate(
                            func.AssemblyName,
                            func.ModuleName,
                            func.Name,
                            func.ArgumentsCount,
                            null,
                            null,
                            OnExceptionEmpty3);
                    }
                }
            }
            catch (Exception exception)
            {
                TelemetryClient client = new TelemetryClient();
                client.TrackException(exception);
            }
        }

        public object OnBeginEmpty(object thisObj)
        {
            return null;
        }

        public object OnEndEmpty(object context, object returnValue, object thisObj)
        {
            return returnValue;
        }

        public void OnExceptionTrackException(object context, object exception, object thisObj)
        {
            Debug.WriteLine("OnExceptionTrackException");
            var excTelemetry = new ExceptionTelemetry((Exception)exception);
            excTelemetry.Properties["this"] = thisObj.ToString();
            this.telemetryClient.TrackException(excTelemetry);
        }

        public object OnBeginEmpty2(object thisObj, object argument)
        {
            return null;
        }

        public void OnExceptionEmpty2(object context, object exception, object thisObj, object argument)
        {
            Debug.WriteLine("OnExceptionTrackException2");
            var excTelemetry = new ExceptionTelemetry((Exception)exception);
            excTelemetry.Properties["this"] = thisObj.ToString();
            this.telemetryClient.TrackException(excTelemetry);
        }

        public void OnExceptionEmpty3(object context, object exception, object thisObj, object argument1, object argument2)
        {
            Debug.WriteLine("OnExceptionTrackException3");
            var excTelemetry = new ExceptionTelemetry((Exception)exception);
            excTelemetry.Properties["this"] = thisObj.ToString();
            this.telemetryClient.TrackException(excTelemetry);
        }
    }
}