namespace Microsoft.LocalForwarder.Test.Library.Inputs.GrpcInput
{
    using LocalForwarder.Library.Inputs.GrpcInput;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using LocalForwarder.Library.Inputs.Contracts;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GrpcAiInputTests
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public async Task GrpcAiInputTests_StartsAndStops()
        {
            // ARRANGE
            int port = Common.GetPort();
            var input = new GrpcAiInput("localhost", port);

            // ACT
            input.Start(null);

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcAiInputTests.DefaultTimeout));

            input.Stop();

            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcAiInputTests.DefaultTimeout));

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GrpcAiInputTests_CantStartWhileRunning()
        {
            // ARRANGE
            int port = Common.GetPort();
            var input = new GrpcAiInput("localhost", port);

            input.Start(null);

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcAiInputTests.DefaultTimeout));

            // ACT
            input.Start(null);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GrpcAiInputTests_CantStopWhileStopped()
        {
            // ARRANGE
            int port = Common.GetPort();
            var input = new GrpcAiInput("localhost", port);
            
            // ACT
            input.Stop();

            // ASSERT
        }

        [TestMethod]
        public async Task GrpcAiInputTests_ReceivesData()
        {
            // ARRANGE
            int batchesReceived = 0;
            TelemetryBatch receivedBatch = null;

            int port = Common.GetPort();
            var input = new GrpcAiInput("localhost", port);
            input.Start(telemetryBatch =>
            {
                batchesReceived++;
                receivedBatch = telemetryBatch;
            });
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcAiInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(true, port);

            // ACT
            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});

            await grpcWriter.Write(batch).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcAiInputTests.DefaultTimeout);

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcAiInputTests.DefaultTimeout));
        }

        [TestMethod]
        public async Task GrpcAiInputTests_ReceivesDataFromMultipleClients()
        {
            // ARRANGE
            int batchesReceived = 0;
            TelemetryBatch receivedBatch = null;
            
            int port = Common.GetPort();
            var input = new GrpcAiInput("localhost", port);
            input.Start(telemetryBatch =>
            {
                Interlocked.Increment(ref batchesReceived);
                receivedBatch = telemetryBatch;
            });
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcAiInputTests.DefaultTimeout));

            // ACT
            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() { Event = new Event() { Name = "Event1" } });

            Parallel.For(0, 1000, new ParallelOptions() {MaxDegreeOfParallelism = 1000}, async i =>
            {
                var grpcWriter = new GrpcWriter(true, port);

                await grpcWriter.Write(batch).ConfigureAwait(false);
            });

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1000 && batchesReceived == 1000, GrpcAiInputTests.DefaultTimeout);

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcAiInputTests.DefaultTimeout));
        }

        [TestMethod]
        public async Task GrpcAiInputTests_StopsWhileWaitingForData()
        {
            // ARRANGE
            int batchesReceived = 0;
            TelemetryBatch receivedBatch = null;

            int port = Common.GetPort();
            var input = new GrpcAiInput("localhost", port);

            input.Start(telemetryBatch =>
            {
                batchesReceived++;
                receivedBatch = telemetryBatch;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcAiInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(true, port);

            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});

            await grpcWriter.Write(batch).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcAiInputTests.DefaultTimeout);

            // ACT
            input.Stop();
            
            // ASSERT
            Common.AssertIsTrueEventually(
                () => !input.IsRunning && input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcAiInputTests.DefaultTimeout);
        }

        [TestMethod]
        public async Task GrpcAiInputTests_StopsAndRestarts()
        {
            // ARRANGE
            int batchesReceived = 0;
            TelemetryBatch receivedBatch = null;

            int port = Common.GetPort();
            var input = new GrpcAiInput("localhost", port);

            input.Start(telemetryBatch =>
            {
                batchesReceived++;
                receivedBatch = telemetryBatch;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcAiInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(true, port);

            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});

            await grpcWriter.Write(batch).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcAiInputTests.DefaultTimeout);

            // ACT
            input.Stop();

            Common.AssertIsTrueEventually(
                () => !input.IsRunning && input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Items.Single().Event.Name == "Event1", GrpcAiInputTests.DefaultTimeout);

            input.Start(telemetryBatch =>
            {
                batchesReceived++;
                receivedBatch = telemetryBatch;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcAiInputTests.DefaultTimeout));

            grpcWriter = new GrpcWriter(true, port);
            batch.Items.Single().Event.Name = "Event2";
            await grpcWriter.Write(batch).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().BatchesReceived == 1 && batchesReceived == 2 &&
                      receivedBatch.Items.Single().Event.Name == "Event2", GrpcAiInputTests.DefaultTimeout);
        }

        [TestMethod]
        public async Task GrpcAiInputTests_HandlesExceptionsInProcessingHandler()
        {
            // ARRANGE
            int port = Common.GetPort();
            var input = new GrpcAiInput("localhost", port);

            input.Start(telemetryBatch => throw new InvalidOperationException());

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcAiInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(true, port);

            TelemetryBatch batch = new TelemetryBatch();
            batch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});

            // ACT
            await grpcWriter.Write(batch).ConfigureAwait(false);

            // ASSERT

            // must have handled the exception by logging it
            // should still be able to process items
            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().BatchesReceived == 0 && input.GetStats().BatchesFailed == 1,
                GrpcAiInputTests.DefaultTimeout);

            await grpcWriter.Write(batch).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().BatchesReceived == 0 && input.GetStats().BatchesFailed == 2,
                GrpcAiInputTests.DefaultTimeout);
        }
    }
}
