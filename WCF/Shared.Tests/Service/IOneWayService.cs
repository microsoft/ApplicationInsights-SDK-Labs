namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    using System;
    using System.ServiceModel;

    [ServiceContract]
    public interface IOneWayService
    {
        [OperationContract(IsOneWay = true)]
        void SuccessfullOneWayCall();

        [OperationContract(IsOneWay = true)]
        void FailureOneWayCall();
    }
}
