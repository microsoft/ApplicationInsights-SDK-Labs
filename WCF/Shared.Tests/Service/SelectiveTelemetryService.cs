using System;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
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
