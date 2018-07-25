namespace Library.Inputs.GrpcInput
{
    using Grpc.Core;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    
    /// <summary>
    /// gRpc-based input
    /// </summary>
    class GrpcInput : TelemetryService.TelemetryServiceBase, IInput
    {
        private CancellationTokenSource cts;
        private Action<TelemetryBatch> onBatchReceived;
        private Server server;
        private InputStats stats;
        private readonly string host;
        private readonly int port;

        public GrpcInput(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public void Start(Action<TelemetryBatch> onBatchReceived)
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Can't Start the input, it's already running"));
            }

            this.stats = new InputStats();
            this.cts = new CancellationTokenSource();
            this.onBatchReceived = onBatchReceived;
            
            try
            {
                this.server = new Server
                {
                    Services = {TelemetryService.BindService(this)},
                    Ports = {new ServerPort(host, this.port, ServerCredentials.Insecure)}
                };

                this.server.Start();

                this.IsRunning = true;
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not initialize the gRPC server. {e.ToString()}"));
            }
        }

        public void Stop()
        {
            if (!this.IsRunning)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Can't Stop the input, it's not currently running"));
            }

            try
            {
                this.server.KillAsync().Wait(TimeSpan.FromSeconds(5));
            }
            finally
            {

                this.cts.Cancel();

                this.IsRunning = false;
            }
        }

        public bool IsRunning { get; private set; }

        public InputStats GetStats()
        {
            return this.stats;
        }

        public override async Task SendTelemetryBatch(IAsyncStreamReader<TelemetryBatch> requestStream,
            IServerStreamWriter<Response> responseStream,
            ServerCallContext context)
        {
            try
            {
                while (await requestStream.MoveNext(this.cts.Token).ConfigureAwait(false))
                {
                    TelemetryBatch batch = requestStream.Current;

                    try
                    {
                        this.onBatchReceived?.Invoke(batch);

                        Interlocked.Increment(ref this.stats.BatchesReceived);
                    }
                    catch (System.Exception e)
                    {
                        // unexpected exception occured while processing the batch
                        Interlocked.Increment(ref this.stats.BatchesFailed);

                        Common.Diagnostics.Log(FormattableString.Invariant($"Unknown exception while processing a batch received through the gRpc input. {e.ToString()}"));
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // we have been stopped
            }
            catch (System.Exception e)
            {
                // unexpected exception occured
                this.Stop();

                Common.Diagnostics.Log(FormattableString.Invariant($"Unknown exception while reading from gRpc stream. {e.ToString()}"));
            }
        }
    }
}