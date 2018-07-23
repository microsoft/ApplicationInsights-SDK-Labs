namespace Test.Library
{
    using System;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Library.Inputs.Contracts;
    using Grpc.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Exception = System.Exception;

    public class GrpcWriter
    {
        private readonly TimeSpan timeout;

        AsyncDuplexStreamingCall<TelemetryBatch, Response> streamingCall;
        private int port;

        public GrpcWriter(int port, TimeSpan timeout)
        {
            this.timeout = timeout;
            this.port = port;

            try
            {
                var channel = new Channel($"127.0.0.1:{this.port}", ChannelCredentials.Insecure);
                var client = new TelemetryService.TelemetryServiceClient(channel);
                this.streamingCall = client.SendTelemetryBatch();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Error initializing the gRpc test client. {e.ToString()}"));
            }

        }

        public async Task Write(TelemetryBatch batch)
        {
            try
            {
                await this.streamingCall.RequestStream.WriteAsync(batch).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Error sending a message via gRpc. {e.ToString()}"));
            }
        }
    }
}
