using System;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    [ServiceTelemetry]
    public class OneWayService : IOneWayService
    {
        public void SuccessfullOneWayCall()
        {
        }
        public void FailureOneWayCall()
        {
            throw new InvalidOperationException("Call failed");
        }
    }
}
