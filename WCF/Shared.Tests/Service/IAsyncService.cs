using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
    }
}
