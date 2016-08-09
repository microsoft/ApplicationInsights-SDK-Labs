using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    [ServiceContract]
    public interface ISimpleService2
    {
        [OperationContract]
        void SampleOperation();
    }
}
