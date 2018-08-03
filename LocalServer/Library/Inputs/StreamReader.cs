namespace Microsoft.LocalForwarder.Library.Inputs
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Exception = System.Exception;

    internal class StreamReader
    {
        /// <summary>
        /// All reads are sequential, so maintain a single buffer for incoming raw batches
        /// Reallocate whenever a larger item comes in
        /// </summary>
        private byte[] incomingBatchRaw = new byte[0];

        private byte[] incomingBatchLengthRaw = new byte[4];

        //public static async Task<TelemetryItemBatch> ReadSocket(NetworkStream stream, Action<int> bytesRead)
        //{
        //    await ReadSocket(stream, incomingBatchLengthRaw, incomingBatchLengthRaw.Length, bytesRead).ConfigureAwait(false);

        //    if (!Serializer.TryReadLengthPrefix(incomingBatchLengthRaw, 0, incomingBatchLengthRaw.Length, PrefixStyle.Fixed32,
        //        out int itemLength))
        //    {
        //        throw new ArgumentException("Could not read the length prefix from the socket");
        //    }

        //    if (itemLength > incomingBatchRaw.Length)
        //    {
        //        incomingBatchRaw = new byte[itemLength];
        //    }

        //    await ReadSocket(stream, incomingBatchRaw, itemLength, bytesRead).ConfigureAwait(false);

        //    using (var ms = new MemoryStream(incomingBatchRaw, 0, itemLength))
        //    {
        //        return Serializer.Deserialize<TelemetryItemBatch>(ms);
        //    }
        //}

        public async Task<TelemetryBatch> ReadPipe(NamedPipeServerStream stream, PipeServerStats stats, CancellationToken ct)
        {
            await StreamReader.ReadPipe(stream, this.incomingBatchLengthRaw, this.incomingBatchLengthRaw.Length, null, ct).ConfigureAwait(false);

            int itemLength = StreamReader.DecodeLengthPrefix(this.incomingBatchLengthRaw);
            
            if (itemLength > this.incomingBatchRaw.Length)
            {
                this.incomingBatchRaw = new byte[itemLength];
            }

            await StreamReader.ReadPipe(stream, this.incomingBatchRaw, itemLength, stats, ct).ConfigureAwait(false);

            return TelemetryBatch.Parser.ParseFrom(this.incomingBatchRaw, 0, itemLength);
        }

        //public static async Task ReadSocket(NetworkStream stream, byte[] bytes, int count, Action<int> bytesRead)
        //{
        //    int bytesReadSoFar = 0;
        //    while (bytesReadSoFar < count)
        //    {
        //        int chunkSize;
        //        try
        //        {
        //            // this is a TCP socket, so ReadAsync will return 0 only when the connection has been terminated from the other side
        //            var ct = new CancellationTokenSource(StreamReader.ReadTimeout);
        //            chunkSize = await stream.ReadAsync(bytes, bytesReadSoFar, count - bytesReadSoFar, ct.Token).ConfigureAwait(false);
        //        }
        //        catch (Exception e)
        //        {
        //            //Console.WriteLine(
        //            //    $"ReadAsync failed. BytesReadOverall: {bytesReadSoFar} {e.GetType().FullName} {e.Message}, {e.InnerException?.Message}");
        //            throw new InvalidOperationException("stream.ReadAsync failed", e);
        //        }

        //        bytesReadSoFar += chunkSize;

        //        bytesRead?.Invoke(chunkSize);

        //        if (chunkSize == 0)
        //        {
        //            // the connection was closed before we received the expected number of bytes
        //            throw new InvalidOperationException(
        //                "Connection was closed before all expected bytes were received");
        //        }
        //    }
        //}

        public static async Task ReadPipe(NamedPipeServerStream stream, byte[] bytes, int count, PipeServerStats stats, CancellationToken ct)
        {
            int bytesReadSoFar = 0;
            while (bytesReadSoFar < count)
            {
                int chunkSize;
                try
                {
                    // this is a pipe, so ReadAsync will return 0 only when the pipe has been closed from the other end
                    chunkSize = await stream.ReadAsync(bytes, bytesReadSoFar, count - bytesReadSoFar, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException e)
                {
                    throw new InvalidOperationException("Reading operation on a pipe was cancelled", e);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Reading operation failed on a pipe.", e);
                }

                bytesReadSoFar += chunkSize;

                if (stats != null)
                {
                    stats.BytesRead += (ulong) chunkSize;
                }

                if (chunkSize == 0)
                {
                    // the connection was closed before we received the expected number of bytes
                    throw new EndOfStreamException("Connection was closed before all expected bytes were received");
                }
            }
        }

        private static int DecodeLengthPrefix(byte[] data)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(FormattableString.Invariant($"Length prefix must be 4 bytes long"));
            }

            // little endian
            return data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
        }
    }
}