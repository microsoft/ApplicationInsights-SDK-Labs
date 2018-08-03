namespace Microsoft.LocalForwarder.Test
{
    using Grpc.Core;
    using LocalForwarder.Library.Inputs.Contracts;
    using Opencensus.Proto.Exporter;
    using System;
    using System.Threading.Tasks;

    public class GrpcWriter
    {
        private readonly bool aiMode;

        AsyncDuplexStreamingCall<TelemetryBatch, AiResponse> aiStreamingCall;
        AsyncDuplexStreamingCall<ExportSpanRequest, ExportSpanResponse> openCensusStreamingCall;
        private int port;

        public GrpcWriter(bool aiMode, int port)
        {
            this.aiMode = aiMode;
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
                    var client = new Export.ExportClient(channel);
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
