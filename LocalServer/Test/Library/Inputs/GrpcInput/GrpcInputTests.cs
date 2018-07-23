namespace Test.Library.Inputs.NamedPipeInput
{
    using global::Library.Inputs.Contracts;
    using global::Library.Inputs.GrpcInput;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    public class GrpcInputTests
    {
        private static readonly Random rand = new Random();
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public async Task GrpcInputTests_StartsAndStops()
        {
            // ARRANGE
            int port = GetPort();
            var input = new GrpcInput(port);

            // ACT
            input.Start(null);

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            input.Stop();

            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcInputTests.DefaultTimeout));

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GrpcInputTests_CantStartWhileRunning()
        {
            // ARRANGE
            int port = GetPort();
            var input = new GrpcInput(port);

            input.Start(null);

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            // ACT
            input.Start(null);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GrpcInputTests_CantStopWhileStopped()
        {
            // ARRANGE
            int port = GetPort();
            var input = new GrpcInput(port);
            
            // ACT
            input.Stop();

            // ASSERT
        }

        [TestMethod]
        public async Task GrpcInputTests_ReceivesData()
        {
            // ARRANGE
            int batchesReceived = 0;
            TelemetryBatch receivedBatch = null;

            int port = GetPort();
            var input = new GrpcInput(port);
            input.Start(telemetryBatch =>
            {
                batchesReceived++;
                receivedBatch = telemetryBatch;
            });
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(port, GrpcInputTests.DefaultTimeout);

            // ACT
            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});

            await grpcWriter.Write(batch).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcInputTests.DefaultTimeout);

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcInputTests.DefaultTimeout));
        }

        [TestMethod]
        public async Task GrpcInputTests_StopsWhileWaitingForData()
        {
            // ARRANGE
            int batchesReceived = 0;
            TelemetryBatch receivedBatch = null;

            int port = GetPort();
            var input = new GrpcInput(port);

            input.Start(telemetryBatch =>
            {
                batchesReceived++;
                receivedBatch = telemetryBatch;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(port, GrpcInputTests.DefaultTimeout);

            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});

            await grpcWriter.Write(batch).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcInputTests.DefaultTimeout);

            // ACT
            input.Stop();
            
            // ASSERT
            Common.AssertIsTrueEventually(
                () => !input.IsRunning && input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcInputTests.DefaultTimeout);
        }

        [TestMethod]
        public async Task GrpcInputTests_StopsAndRestarts()
        {
            // ARRANGE
            int batchesReceived = 0;
            TelemetryBatch receivedBatch = null;

            int port = GetPort();
            var input = new GrpcInput(port);

            input.Start(telemetryBatch =>
            {
                batchesReceived++;
                receivedBatch = telemetryBatch;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(port, GrpcInputTests.DefaultTimeout);

            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});

            await grpcWriter.Write(batch).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcInputTests.DefaultTimeout);

            // ACT
            input.Stop();

            Common.AssertIsTrueEventually(
                () => !input.IsRunning && input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcInputTests.DefaultTimeout);

            input.Start(telemetryBatch =>
            {
                batchesReceived++;
                receivedBatch = telemetryBatch;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            grpcWriter = new GrpcWriter(port, GrpcInputTests.DefaultTimeout);
            batch.Items.Single().Event.Name = "Event2";
            await grpcWriter.Write(batch).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().BatchesReceived == 1 && batchesReceived == 2 &&
                      receivedBatch.Items.Single().Event.Name == "Event2", GrpcInputTests.DefaultTimeout);
        }

        [TestMethod]
        public async Task GrpcInputTests_HandlesExceptionsInProcessingHandler()
        {
            // ARRANGE
            int port = GetPort();
            var input = new GrpcInput(port);

            input.Start(telemetryBatch => throw new InvalidOperationException());

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(port, GrpcInputTests.DefaultTimeout);

            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});

            // ACT
            await grpcWriter.Write(batch).ConfigureAwait(false);

            // ASSERT

            // must have handled the exception by logging it
            // should still be able to process items
            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().BatchesReceived == 0 && input.GetStats().BatchesFailed == 1,
                GrpcInputTests.DefaultTimeout);

            await grpcWriter.Write(batch).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().BatchesReceived == 0 && input.GetStats().BatchesFailed == 2,
                GrpcInputTests.DefaultTimeout);
        }

        private static int GetPort()
        {
            // dynamic port range
            return rand.Next(49152, 65535);
        }
    }
}
