using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    [ServiceContract]
    public interface IOneWayService
    {
        [OperationContract(IsOneWay = true)]
        void SuccessfullOneWayCall();
        [OperationContract(IsOneWay = true)]
        void FailureOneWayCall();
    }
}
