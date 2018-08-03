namespace Microsoft.LocalForwarder.Test.Library.Inputs
{
    using System.Reflection;
    using System.Threading.Tasks;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class StreamReaderTests
    {
        [TestMethod]
        public async Task StreamReaderTests_DecodeLengthPrefix()
        {
            // ARRANGE
            int length = 1493809221;
            byte[] encodedLengthPrefix = Common.EncodeLengthPrefix(length);

            // ACT
            int decodedLength = (int) typeof(global::Microsoft.LocalForwarder.Library.Inputs.StreamReader).InvokeMember("DecodeLengthPrefix",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new[] {encodedLengthPrefix});

            // ASSERT
            Assert.AreEqual(length, decodedLength);
        }

    }
}