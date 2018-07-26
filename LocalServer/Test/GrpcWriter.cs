namespace Test.Library
{
    using global::Library.Inputs.Contracts;
    using Grpc.Core;
    using System;
    using System.Threading.Tasks;
    using Opencensus.Proto.Exporter;

    public class GrpcWriter
    {
        private readonly bool aiMode;
        private readonly TimeSpan timeout;

        AsyncDuplexStreamingCall<TelemetryBatch, AiResponse> aiStreamingCall;
        AsyncDuplexStreamingCall<ExportSpanRequest, ExportSpanResponse> openCensusStreamingCall;
        private int port;

        public GrpcWriter(bool aiMode, int port, TimeSpan timeout)
        {
            this.aiMode = aiMode;
            this.timeout = timeout;
            this.port = port;

            try
            {
                var channel = new Channel($"127.0.0.1:{this.port}", ChannelCredentials.Insecure);

                if (aiMode)
                {
                    var client = new AITelemetryService.AITelemetryServiceClient(channel);
                    this.aiStreamingCall = client.SendTelemetryBatch();
                }
                else
                {
                    // OpenCensus
                    var client = new OpenCensusExport.OpenCensusExportClient(channel);
                    this.openCensusStreamingCall = client.ExportSpan();
                }
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Error initializing the gRpc test client. {e.ToString()}"));
            }

        }

        public async Task Write(TelemetryBatch batch)
        {
            if (!this.aiMode)
            {
                throw new InvalidOperationException("Incorrect mode");
            }

            try
            {
                await this.aiStreamingCall.RequestStream.WriteAsync(batch).ConfigureAwait(false);
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Error sending a message via gRpc. {e.ToString()}"));
            }
        }

        public async Task Write(ExportSpanRequest batch)
        {
            if (this.aiMode)
            {
                throw new InvalidOperationException("Incorrect mode");
            }

            try
            {
                await this.openCensusStreamingCall.RequestStream.WriteAsync(batch).ConfigureAwait(false);
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Error sending a message via gRpc. {e.ToString()}"));
            }
        }
    }
}
