using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    [ServiceContract]
    public interface IAsyncService
    {
        [OperationContract]
        Task<String> GetDataAsync();
        [OperationContract]
        Task<String> FailWithFaultAsync();
        [OperationContract]
        Task<String> FailWithExceptionAsync();
        [OperationContract]
        Task<String> WriteDependencyEventAsync();
    }
}
