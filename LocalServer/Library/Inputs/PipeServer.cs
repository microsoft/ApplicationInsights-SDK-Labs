namespace Microsoft.LocalForwarder.Library.Inputs
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Contracts;
    using Exception = System.Exception;

    /// <summary>
    /// A PipeServer is responsible for talking to a single client connected to a Named Pipe.
    /// </summary>
    class PipeServer
    {
        private const string NamedPipeName = "MsftLocalServer";

        private NamedPipeServerStream pipe;
        private StreamReader reader;
        private PipeServerStats stats;

        private CancellationTokenSource cts;

        private volatile bool isRunning;

        public bool IsRunning
        {
            get => this.isRunning;
            private set => this.isRunning = value;
        }

        public PipeServer()
        {
            this.IsRunning = false;
        }

        public async Task Start(Func<PipeServer, Task> onClientConnected, Func<PipeServer, Task> onClientDisconnected, Func<PipeServer, TelemetryBatch, Task> onBatchReceived)
        {
            try
            {
                if (this.IsRunning)
                {
                    throw new InvalidOperationException(FormattableString.Invariant($"PipeServer is already running, can't start it."));
                }

                this.cts = new CancellationTokenSource();
                this.stats = new PipeServerStats();
                this.reader = new StreamReader();

                // open the pipe
                try
                {
                    this.pipe = new NamedPipeServerStream(PipeServer.NamedPipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Could not initialize a pipe", e);
                }

                // listen for a client
                try
                {
                    this.IsRunning = true;
                    await this.pipe.WaitForConnectionAsync(this.cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // we were stopped while waiting for the client
                    // just exit the listening loop
                    return;
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Could not start listening on a pipe", e);
                }

                // a client has connected
                this.stats.IsClientConnected = true;

                if (onClientConnected != null)
                {
                    await onClientConnected(this).ConfigureAwait(false);
                }

                while (!this.cts.IsCancellationRequested)
                {
                    TelemetryBatch telemetryItemBatch;

                    try
                    {
                        telemetryItemBatch = await this.reader.ReadPipe(this.pipe, this.stats, this.cts.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        // we were stopped
                        // just exit the listening loop
                        return;
                    }
                    catch (EndOfStreamException e)
                    {
                        // the pipe was closed on the other end
                        if (onClientDisconnected != null)
                        {
                            await onClientDisconnected(this).ConfigureAwait(false);
                        }

                        Diagnostics.LogError(FormattableString.Invariant($"The pipe was closed on the other end. {e}"));

                        break;
                    }
                    catch (Exception e)
                    {
                        Diagnostics.LogError(FormattableString.Invariant($"Reading the pipe has failed. Closing the connection. {e}"));

                        break;
                    }

                    if (onBatchReceived != null)
                    {
                        await onBatchReceived(this, telemetryItemBatch).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                // we are out of the receiving loop, meaning the client has closed the pipe from the other end
                // or we got stopped, or something went terribly wrong
                try
                {
                    if (this.pipe?.IsConnected == true)
                    {
                        this.pipe.Disconnect();
                    }
                }
                catch (Exception e)
                {
                    Diagnostics.LogError(FormattableString.Invariant($"Error while attempting to disconnect the pipe. {e}"));
                }

                this.pipe?.Dispose();

                this.IsRunning = false;
            }
        }

        public void Stop()
        {
            try
            {
                if (!this.IsRunning)
                {
                    throw new InvalidOperationException(FormattableString.Invariant($"PipeServer is not currently running, can't stop it."));
                }

                if (this.cts?.IsCancellationRequested == false)
                {
                    this.cts.Cancel();
                }
            }
            catch (Exception e)
            {
                // something went wrong while traing to cancel the operation, let's at least dispose of the pipe
                
                Diagnostics.LogError(FormattableString.Invariant($"Error while attempting to cancel a pipe operation. {e}"));

                this.pipe.Dispose();
            }
        }

        public PipeServerStats GetStats()
        {
            return this.stats;
        }

    }
}
