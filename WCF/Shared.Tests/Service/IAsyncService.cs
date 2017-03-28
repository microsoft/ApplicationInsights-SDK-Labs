namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    using System;
    using System.ServiceModel;
    using System.Threading.Tasks;

    [ServiceContract]
    public interface IAsyncService
    {
        [OperationContract]
        Task<string> GetDataAsync();
        [OperationContract]
        Task<string> FailWithFaultAsync();
        [OperationContract]
        Task<string> FailWithExceptionAsync();
        [OperationContract]
        Task<string> WriteDependencyEventAsync();
    }
}
