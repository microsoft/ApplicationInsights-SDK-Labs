namespace Microsoft.LocalForwarder.Library.Inputs.GrpcInput
{
    using Contracts;

    class GrpcAiInput : GrpcInput<TelemetryBatch, AiResponse>
    {
        public GrpcAiInput(string host, int port) : base(host, port)
        {
        }
    }
}