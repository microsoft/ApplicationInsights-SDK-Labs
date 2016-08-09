using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
#if NET45
    [ServiceTelemetry]
    public class AsyncService : IAsyncService
    {
        public async Task<String> GetDataAsync()
        {
            await Task.Delay(200);
            return "Hello";
        }

        public async Task<String> FailWithFaultAsync()
        {
            await Task.Delay(200);
            throw new FaultException("Call failed");
        }

        public async Task<String> FailWithExceptionAsync()
        {
            await Task.Delay(200);
            throw new InvalidOperationException();
        }
    }
#endif // NET45
}
