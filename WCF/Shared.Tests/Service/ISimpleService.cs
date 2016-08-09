using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    [ServiceContract]
    public interface ISimpleService
    {
        [OperationContract]
        String GetSimpleData();
        [OperationContract]
        void CallFailsWithFault();
        [OperationContract]
        void CallFailsWithTypedFault();
        [OperationContract]
        void CallFailsWithException();
        [OperationContract]
        void CallWritesExceptionEvent();
        [OperationContract]
        void CallMarksRequestAsFailed();
        [OperationContract(Action="*")]
        void CatchAllOperation();
        [OperationContract]
        void CallThatEmitsEvent();
        [OperationContract]
        void CallAnotherServiceAndLeakOperationContext(String address);
        [OperationContract]
        bool CallIsClientSideContext();
    }
}
