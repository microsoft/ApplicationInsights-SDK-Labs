namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    using System;

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
