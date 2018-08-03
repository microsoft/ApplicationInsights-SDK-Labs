namespace Microsoft.LocalForwarder.Library.Inputs.GrpcInput
{
    using Opencensus.Proto.Exporter;

    class GrpcOpenCensusInput : GrpcInput<ExportSpanRequest, ExportSpanResponse>
    {
        public GrpcOpenCensusInput(string host, int port) : base(host, port)
        {
        }
    }
}