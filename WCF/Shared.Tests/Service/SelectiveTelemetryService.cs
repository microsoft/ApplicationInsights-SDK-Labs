namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    using System;

    [ServiceTelemetry]
    public class SelectiveTelemetryService : ISelectiveTelemetryService
    {
        public void OperationWithoutTelemetry()
        {
        }

        public void OperationWithTelemetry()
        {
        }
    }
}
