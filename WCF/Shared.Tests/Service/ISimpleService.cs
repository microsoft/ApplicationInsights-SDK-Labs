namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    using System;
    using System.ServiceModel;

    [ServiceContract]
    public interface ISimpleService
    {
        [OperationContract]
        string GetSimpleData();

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

        [OperationContract(Action = "*")]
        void CatchAllOperation();

        [OperationContract]
        void CallThatEmitsEvent();

        [OperationContract]
        void CallAnotherServiceAndLeakOperationContext(string address);

        [OperationContract]
        bool CallIsClientSideContext();
    }
}
