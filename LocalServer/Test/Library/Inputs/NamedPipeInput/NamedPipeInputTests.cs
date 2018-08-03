namespace Microsoft.LocalForwarder.Test.Library.Inputs.NamedPipeInput
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Google.Protobuf;
    using LocalForwarder.Library.Inputs.Contracts;
    using LocalForwarder.Library.Inputs.NamedPipeInput;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NamedPipeInputTests
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public async Task NamedPipeInputTests_StartsAndStops()
        {
            // ARRANGE
            var input = new NamedPipeInput();

            // ACT
            input.Start(null);

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, NamedPipeInputTests.DefaultTimeout));

            input.Stop();

            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, NamedPipeInputTests.DefaultTimeout));

            // ASSERT
        }

        [TestMethod]
        public async Task NamedPipeInputTests_AcceptsConnection()
        {
            // ARRANGE
            var input = new NamedPipeInput();
            input.Start(null);
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, NamedPipeInputTests.DefaultTimeout));

            // ACT
            var pipeWriter = new PipeWriter(NamedPipeInputTests.DefaultTimeout);
            await pipeWriter.Start().ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(() => input.GetStats().ConnectionCount == 1,
                NamedPipeInputTests.DefaultTimeout);

            pipeWriter.Stop();

            input.Stop();

            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, NamedPipeInputTests.DefaultTimeout));
        }

        [TestMethod]
        public async Task NamedPipeInputTests_ReceivesData()
        {
            // ARRANGE
            int batchesReceived = 0;
            TelemetryBatch receivedBatch = null;

            var input = new NamedPipeInput();
            input.Start((telemetryBatch) =>
            {
                batchesReceived++;
                receivedBatch = telemetryBatch;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, NamedPipeInputTests.DefaultTimeout));

            var pipeWriter = new PipeWriter(NamedPipeInputTests.DefaultTimeout);
            await pipeWriter.Start().ConfigureAwait(false);

            // ACT
            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});

            using (var ms = new MemoryStream())
            {
                batch.WriteTo(ms);

                await ms.FlushAsync().ConfigureAwait(false);

                await pipeWriter.Write(Common.EncodeLengthPrefix(batch.CalculateSize())).ConfigureAwait(false);
                await pipeWriter.Write(ms.ToArray()).ConfigureAwait(false);
            }

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.GetStats().ConnectionCount == 1 && input.GetStats().BatchesReceived == 1 &&
                      batchesReceived == 1 && receivedBatch.Items.Single().Event.Name == "Event1",
                NamedPipeInputTests.DefaultTimeout);

            pipeWriter.Stop();

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, NamedPipeInputTests.DefaultTimeout));
        }

        /*
         * PipeInput was deprioritized and test coverage left incomplete. Complete coverage before using
         */
        [Ignore]
        [TestMethod()]
        public void NamedPipeInputTests_IncompleteCoverage()
        {
        }
    }
}
