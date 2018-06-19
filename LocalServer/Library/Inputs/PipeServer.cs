namespace Library.Inputs
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;

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


        public PipeServer()
        {
        }

        public async Task Start(Func<PipeServer, Task> onClientConnected, Func<PipeServer, Task> onClientDisconnected, Func<PipeServer, Contracts.TelemetryBatch, Task> onBatchReceived)
        {
            try
            {
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
                    await this.pipe.WaitForConnectionAsync().ConfigureAwait(false);
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

                while (true)
                {
                    Contracts.TelemetryBatch telemetryItemBatch;

                    try
                    {
                        telemetryItemBatch = await this.reader.ReadPipe(this.pipe, this.stats, this.cts.Token).ConfigureAwait(false);
                    }
                    catch (EndOfStreamException e)
                    {
                        // the pipe was closed on the other end
                        if (onClientDisconnected != null)
                        {
                            await onClientDisconnected(this).ConfigureAwait(false);
                        }

                        Common.Diagnostics.Log(FormattableString.Invariant($"The pipe was closed on the other end. {e}"));

                        break;
                    }
                    catch (Exception e)
                    {
                        Common.Diagnostics.Log(FormattableString.Invariant($"Reading the pipe has failed. Closing the connection. {e}"));

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
                this.pipe?.Dispose();
            }
        }

        public void Stop()
        {
            try
            {
                this.cts.Cancel();

                if (this.pipe.IsConnected)
                {
                    this.pipe.Disconnect();
                }
            }
            catch (Exception e)
            {
                Common.Diagnostics.Log(FormattableString.Invariant($"Error while attempting to close the pipe. {e}"));
            }
            finally
            {
                this.pipe.Dispose();
            }
        }

        public PipeServerStats GetStats()
        {
            return this.stats;
        }

    }
}
