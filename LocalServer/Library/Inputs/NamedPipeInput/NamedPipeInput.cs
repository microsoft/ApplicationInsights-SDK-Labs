namespace Microsoft.LocalForwarder.Library.Inputs.NamedPipeInput
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Exception = System.Exception;

    /// <summary>
    /// Named pipe-based input
    /// </summary>
    /// <remarks>NamedPipeInput maintains a collection of PipeServer's. Each PipeServer supports a single connection, but they all share the same OS pipe.</remarks>
    class NamedPipeInput : IInput
    {
        private readonly List<PipeServer> pipeServers = new List<PipeServer>();

        private InputStats stats;

        private Action<TelemetryBatch> onBatchReceived;

        private CancellationTokenSource cts;

        private volatile bool isRunning;

        private volatile bool isStoppingInput = false;

        public NamedPipeInput()
        {
            this.IsRunning = false;
        }

        /// <summary>
        /// Starts listening for data.
        /// </summary>
        /// <param name="onBatchReceived">A callback to be invoked every time there is a new incoming telemetry batch. No guarantees are provided as to which thread the callback is called on.</param>
        public void Start(Action<TelemetryBatch> onBatchReceived)
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException(FormattableString.Invariant($"NamedPipeInput is already running, can't start it."));
            }

            this.onBatchReceived = onBatchReceived;

            this.cts = new CancellationTokenSource();

            this.stats = new InputStats();

            try
            {
               // start the first PipeServer, it will wait for the first connection
                var firstPipeServer = new PipeServer();

                lock (this.pipeServers)
                {
                    this.pipeServers.Add(firstPipeServer);
                }

                Task.Run(() =>
                {
                    // call pipe server's Start() and mark ourselves as running before it completes (after the first await), so no await in the call below
#pragma warning disable 4014
                    firstPipeServer.Start(this.OnClientConnected, this.OnClientDisconnected, this.OnBatchReceived);
#pragma warning restore 4014

                    this.IsRunning = true;
                });
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to start the first pipe server. This entire pipe input will not be able to receive data.", e);
            }
        }

        public void Stop()
        {
            try
            {
                if (!this.IsRunning)
                {
                    throw new InvalidOperationException(FormattableString.Invariant($"NamedPipeInput is not currently running, can't stop it."));
                }

                this.isStoppingInput = true;

                var errorMessages = new List<string>();
                Action shutDownAllServers = () =>
                {
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
                                    FormattableString.Invariant(
                                        $"Failed to stop one of the pipe servers: {e.ToString()}"));
                            }
                        }
                    }
                };

                shutDownAllServers();

                if (!SpinWait.SpinUntil(() =>
                {
                    lock (this.pipeServers)
                    {
                        return this.pipeServers.All(ps => !ps.IsRunning);
                    }
                }, TimeSpan.FromSeconds(5)))
                {
                    // it appears there are still running pipe servers in this.pipeServers collection
                    // (it is possible due to race condition with creating new servers in OnClientConnected callback handler)
                    // now that we're under the this.isStoppingInput flag, let threads cool down and stop the remaining ones
                    Thread.Sleep(TimeSpan.FromSeconds(3));

                    shutDownAllServers();

                    if (!SpinWait.SpinUntil(() =>
                    {
                        lock (this.pipeServers)
                        {
                            return this.pipeServers.All(ps => !ps.IsRunning);
                        }
                    }, TimeSpan.FromSeconds(5)))
                    {
                        // no luck again, report an issue
                        throw new InvalidOperationException(
                            FormattableString.Invariant(
                                $"Failed to stop some of the pipe servers within the alloted time period. {string.Join(", ", errorMessages)}"));
                    }
                }

                if (errorMessages.Any())
                {
                    throw new InvalidOperationException(
                        FormattableString.Invariant(
                            $"Failed to stop some of the pipe servers. {string.Join(", ", errorMessages)}"));
                }
            }
            finally
            {
                lock (this.pipeServers)
                {
                    this.pipeServers.Clear();
                }

                this.IsRunning = false;
                this.isStoppingInput = false;
            }
        }

        public bool IsRunning
        {
            get => this.isRunning;
            private set => this.isRunning = value;
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
                if (this.isStoppingInput)
                {
                    // we are being stopped, don't do anything
                    return;
                }

                var nextPipeServer = new PipeServer();

                lock (this.pipeServers)
                {
                    this.pipeServers.Add(nextPipeServer);
                }

                // no await here, we don't want to wait past the first await (when it starts listening)
                nextPipeServer.Start(this.OnClientConnected, this.OnClientDisconnected, this.OnBatchReceived);

                Interlocked.Increment(ref this.stats.ConnectionCount);
            }
            catch (Exception e)
            {
                // failed to start the next pipe server
                // this is unexpected, could be that the OS limit is reached
                // keep serving existing clients, but no new servers will start. Ever.
                lock (this.pipeServers)
                {
                    Diagnostics.LogError(FormattableString.Invariant(
                        $"Could not start the next pipe server. Pipe server count: {this.pipeServers.Count}. {e.ToString()}"));
                }
            }
        }

        private async Task OnClientDisconnected(PipeServer pipeServer)
        {
            // the client has disconnected, dispose of this server

            if (this.isStoppingInput)
            {
                // we are being stopped, don't do anything
                return;
            }

            lock (this.pipeServers)
            {
                this.pipeServers.Remove(pipeServer);
            }

            try
            {
                pipeServer.Stop();

                Interlocked.Increment(ref this.stats.ConnectionCount);
            }
            catch (Exception e)
            {
                // this is unexpected, not much we can do
                Diagnostics.LogError(
                    FormattableString.Invariant($"Error stopping a pipe server. {e.ToString()}"));
            }
        }

        private async Task OnBatchReceived(PipeServer pipeServer, TelemetryBatch batch)
        {
            Interlocked.Increment(ref this.stats.BatchesReceived);

            try
            {
                this.onBatchReceived?.Invoke(batch);
            }
            catch (Exception e)
            {
                // our client has thrown in a callback
                // log and carry on
                Diagnostics.LogError(
                    FormattableString.Invariant($"OnBatchReceived callback threw. {e.ToString()}"));
            }
        }
    }
}
