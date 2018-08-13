namespace Microsoft.LocalForwarder.Test
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using VisualStudio.TestTools.UnitTesting;

    public static class Common
    {
        private static readonly Random rand = new Random();

        public static byte[] EncodeLengthPrefix(int length)
        {
            // little endian
            //value = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
            // xx xx xx xx
            return new byte[] {(byte)(length & 0x000000ff), (byte)((length & 0x0000ff00) >> 8), (byte)((length & 0x00ff0000) >> 16), (byte)((length & 0xff000000) >> 24) };
        }

        public static void AssertIsTrueEventually(Func<bool> condition, TimeSpan? timeout = null)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(10);
            Assert.IsTrue(SpinWait.SpinUntil(condition, timeout.Value));
        }

        public static void AssertIsFalseEventually(Func<bool> condition, TimeSpan? timeout = null)
        {
            timeout = timeout ?? Timeout.InfiniteTimeSpan;
            Assert.IsFalse(SpinWait.SpinUntil(condition, timeout.Value));
        }

        public static int GetPort()
        {
            // dynamic port range
            return rand.Next(49152, 65535);
        }

        public static TelemetryClient SetupStubTelemetryClient(out ConcurrentQueue<ITelemetry> sentItems)
        {
            var configuration = new TelemetryConfiguration();
            var queue = new ConcurrentQueue<ITelemetry>();

            var channel = new StubTelemetryChannel
            {
                OnSend = delegate (ITelemetry t)
                {
                    queue.Enqueue(t);
                }
            };

            sentItems = queue;

            configuration.TelemetryChannel = channel;

            return new TelemetryClient(configuration);
        }
    }
}
