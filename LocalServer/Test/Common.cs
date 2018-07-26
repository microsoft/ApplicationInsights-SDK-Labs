namespace Test.Library
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Threading;

    public static class Common
    {
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
