namespace Library.Inputs.NamedPipeInput
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Named pipe-based input
    /// </summary>
    /// <remarks>NamedPipeInput maintains a collection of PipeServer's. Each PipeServer supports a single connection, but they all share the same OS pipe.</remarks>
    class NamedPipeInput : IInput
    {
        private readonly List<PipeServer> pipeServers = new List<PipeServer>();

        private InputStats stats;

        private Action<Contracts.TelemetryBatch> onBatchReceived;

        /// <summary>
        /// Starts listening for data.
        /// </summary>
        /// <param name="onBatchReceived">A callback to be invoked every time there is a new incoming telemetry batch. No guarantees are provided as to which thread the callback is called on.</param>
        public async Task Start(Action<Contracts.TelemetryBatch> onBatchReceived)
        {
            this.onBatchReceived = onBatchReceived;

            this.stats = new InputStats();

            try
            {
               // start the first PipeServer, it will wait for the first connection
                var firstPipeServer = new PipeServer();

                lock (this.pipeServers)
                {
                    this.pipeServers.Add(firstPipeServer);
                }

                await firstPipeServer.Start(this.OnClientConnected, this.OnClientDisconnected, this.OnBatchReceived).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to start the first pipe server. This entire pipe input will not be able to receive data.", e);
            }
        }

        public void Stop()
        {
            var errorMessages = new List<string>();
            lock (this.pipeServers)
            {
                foreach (var pipeServer in this.pipeServers)
                {
                    try
                    {
                        pipeServer.Stop();
                    }
                    catch (Exception e)
                    {
                        errorMessages.Add(
                            FormattableString.Invariant($"Failed to stop one of the pipe servers: {e.ToString()}"));
                    }
                }

                this.pipeServers.Clear();
            }

            if (errorMessages.Any())
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant(
                        $"Failed to stop some of the pipe servers. {string.Join(", ", errorMessages)}"));
            }
        }

        public InputStats GetStats()
        {
            return this.stats;
        }

        private async Task OnClientConnected(PipeServer pipeServer)
        {
            // a client has connected, we need to start one more server to listen for the next client
            try
            {
                var nextPipeServer = new PipeServer();

                lock (this.pipeServers)
                {
                    this.pipeServers.Add(nextPipeServer);
                }

                await nextPipeServer.Start(this.OnClientConnected, this.OnClientDisconnected, this.OnBatchReceived).ConfigureAwait(false);

                this.stats.ConnectionCount++;
            }
            catch (Exception e)
            {
                // failed to start the next pipe server
                // this is unexpected, could be that the OS limit is reached
                // keep serving existing clients, but no new servers will start. Ever.
                lock (this.pipeServers)
                {
                    Common.Diagnostics.Log(FormattableString.Invariant(
                        $"Could not start the next pipe server. Pipe server count: {this.pipeServers.Count}. {e.ToString()}"));
                }
            }
        }

        private async Task OnClientDisconnected(PipeServer pipeServer)
        {
            // the client has disconnected, dispose of this server

            lock (this.pipeServers)
            {
                this.pipeServers.Remove(pipeServer);
            }

            try
            {
                pipeServer.Stop();

                this.stats.ConnectionCount--;
            }
            catch (Exception e)
            {
                // this is unexpected, not much we can do
                Common.Diagnostics.Log(
                    FormattableString.Invariant($"Error stopping a pipe server. {e.ToString()}"));
            }
        }

        private async Task OnBatchReceived(PipeServer pipeServer, Contracts.TelemetryBatch batch)
        {
            this.stats.BatchesReceived++;

            try
            {
                this.onBatchReceived?.Invoke(batch);
            }
            catch (Exception e)
            {
                // our client has thrown in a callback
                // log and carry on
                Common.Diagnostics.Log(
                    FormattableString.Invariant($"OnBatchReceived callback threw. {e.ToString()}"));
            }
        }
    }
}
