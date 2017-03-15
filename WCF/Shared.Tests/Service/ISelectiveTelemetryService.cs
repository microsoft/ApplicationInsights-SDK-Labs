namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    using System;
    using System.ServiceModel;

    [ServiceContract]
    public interface ISelectiveTelemetryService
    {
        [OperationContract, OperationTelemetry]
        void OperationWithTelemetry();
        [OperationContract]
        void OperationWithoutTelemetry();
    }
}
