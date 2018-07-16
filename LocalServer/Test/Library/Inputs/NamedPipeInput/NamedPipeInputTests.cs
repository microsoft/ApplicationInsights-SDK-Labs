namespace Test.Library.Inputs.NamedPipeInput
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Pipes;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Library.Inputs.NamedPipeInput;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class NamedPipeInputTests
    {
        [TestMethod]
        public async Task NamedPipeInputTests_StartsAndStops()
        {
            // ARRANGE
            var input = new NamedPipeInput();

            // ACT
            input.Start(null);

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, TimeSpan.FromSeconds(5)));

            input.Stop();

            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, TimeSpan.FromSeconds(5)));

            // ASSERT
        }

        [TestMethod]
        public async Task NamedPipeInputTests_AcceptsConnection()
        {
            // ARRANGE
            var input = new NamedPipeInput();
            input.Start(null);
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, TimeSpan.FromSeconds(5)));

            // ACT
            var pipeWriter = new PipeWriter();
            await pipeWriter.Start().ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(() => input.GetStats().ConnectionCount == 1, TimeSpan.FromSeconds(5));

            await pipeWriter.Stop().ConfigureAwait(false);

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, TimeSpan.FromSeconds(5)));
        }

        [TestMethod]
        public async Task NamedPipeInputTests_ReceivesData()
        {
            // ARRANGE
            var input = new NamedPipeInput();
            input.Start(null);
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, TimeSpan.FromSeconds(5)));

            var pipeWriter = new PipeWriter();
            await pipeWriter.Start().ConfigureAwait(false);

            // ACT
            var message = new byte[] { 1, 2, 7 };
            var lengthPrefix = Common.EncodeLengthPrefix(message.Length);
            await pipeWriter.Write(lengthPrefix).ConfigureAwait(false);
            await pipeWriter.Write(message).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(() => input.GetStats().ConnectionCount == 1 && input.GetStats().BatchesReceived == 1, TimeSpan.FromSeconds(5));
            
            await pipeWriter.Stop().ConfigureAwait(false);

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, TimeSpan.FromSeconds(5)));
        }
        #region Helpers
        #endregion
    }
}
