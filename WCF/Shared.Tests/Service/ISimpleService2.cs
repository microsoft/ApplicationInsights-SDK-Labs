namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    using System;
    using System.ServiceModel;

    [ServiceContract]
    public interface ISimpleService2
    {
        [OperationContract]
        void SampleOperation();
    }
}
