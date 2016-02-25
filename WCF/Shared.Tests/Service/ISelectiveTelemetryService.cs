using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    [ServiceContract]
    public interface ISelectiveTelemetryService
    {
        [OperationContract, OperationTelemetry]
        void OperationWithTelemetry();
        [OperationContract]
        void OperationWithoutTelemetry();
    }
}
