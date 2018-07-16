namespace Test.Library
{
    using System;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class PipeWriter
    {
        private NamedPipeClientStream pipe;

        public PipeWriter()
        {
            NamedPipeClientStream pipe;
            try
            {
                this.pipe = new NamedPipeClientStream(".", "MsftLocalServer",
                    PipeDirection.Out, PipeOptions.Asynchronous);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error initializing the pipe. " + e.Message, e);
            }

        }

        public async Task Start()
        {
            var ct = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await this.pipe.ConnectAsync(ct.Token).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            this.pipe?.WaitForPipeDrain();
            this.pipe?.Dispose();
        }

        public async Task Write(byte[] message)
        {
            try
            {
                // Send the message to the connected pipe
                await this.pipe.WriteAsync(message, 0, message.Length, new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error sending a message", e);
            }
        }

        public static byte[] EncodeLengthPrefix(int length)
        {
            // little endian
            //value = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
            // xx xx xx xx
            return new byte[] {(byte)(length & 0x000000ff), (byte)((length & 0x0000ff00) >> 8), (byte)((length & 0x00ff0000) >> 16), (byte)((length & 0xff000000) >> 24) };
        }

        public static void AssertIsTrueEventually(Func<bool> condition, TimeSpan? timeout = null)
        {
            timeout = timeout ?? Timeout.InfiniteTimeSpan;
            Assert.IsTrue(SpinWait.SpinUntil(condition, timeout.Value));
        }
    }
}
