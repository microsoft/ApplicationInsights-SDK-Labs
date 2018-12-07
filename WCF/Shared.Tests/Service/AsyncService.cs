namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.Threading.Tasks;

#if NET45
    [ServiceTelemetry]
    public class AsyncService : IAsyncService
    {
        public async Task<string> GetDataAsync()
        {
            await Task.Delay(200);
            return "Hello";
        }

        public async Task<string> FailWithFaultAsync()
        {
            await Task.Delay(200);
            throw new FaultException("Call failed");
        }

        public async Task<string> FailWithExceptionAsync()
        {
            await Task.Delay(200);
            throw new InvalidOperationException();
        }

        public async Task<string> WriteDependencyEventAsync()
        {
            var tempFile = Path.GetTempFileName();
            DateTimeOffset start = DateTimeOffset.Now;
            using (var file = File.CreateText(tempFile))
            {
                for (int i = 0; i < 10; i++)
                {
                    await file.WriteLineAsync("This is a line " + i);
                    TelemetryClient client = new TelemetryClient();
                    client.TrackDependency("File", tempFile, start, TimeSpan.FromSeconds(1), true);
                }
            }

            File.Delete(tempFile);
            return "Some value";
        }
    }
#endif // NET45
}
