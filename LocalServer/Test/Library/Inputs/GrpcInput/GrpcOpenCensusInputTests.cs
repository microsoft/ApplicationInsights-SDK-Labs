namespace Microsoft.LocalForwarder.Test.Library.Inputs.GrpcInput
{
    using LocalForwarder.Library.Inputs.GrpcInput;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Opencensus.Proto.Exporter;
    using Opencensus.Proto.Trace;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GrpcOpenCensusInputTests
    {
        private static readonly Random rand = new Random();
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public async Task GrpcOpenCensusInputTests_StartsAndStops()
        {
            // ARRANGE
            int port = GetPort();
            var input = new GrpcOpenCensusInput("localhost", port);

            // ACT
            input.Start(null);

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));

            input.Stop();

            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GrpcOpenCensusInputTests_CantStartWhileRunning()
        {
            // ARRANGE
            int port = GetPort();
            var input = new GrpcOpenCensusInput("localhost", port);

            input.Start(null);

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));

            // ACT
            input.Start(null);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task GrpcOpenCensusInputTests_CantStopWhileStopped()
        {
            // ARRANGE
            int port = GetPort();
            var input = new GrpcOpenCensusInput("localhost", port);
            
            // ACT
            input.Stop();

            // ASSERT
        }

        [TestMethod]
        public async Task GrpcOpenCensusInputTests_ReceivesData()
        {
            // ARRANGE
            int batchesReceived = 0;
            ExportSpanRequest receivedBatch = null;

            int port = GetPort();
            var input = new GrpcOpenCensusInput("localhost", port);
            input.Start(exportSpanRequest =>
            {
                batchesReceived++;
                receivedBatch = exportSpanRequest;
            });
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(false, port);

            // ACT
            ExportSpanRequest batch = new ExportSpanRequest();
            batch.Spans.Add(new Span() {Name = new TruncatableString() {Value = "Event1"}});
            
            await grpcWriter.Write(batch).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Spans.Single().Name.Value == "Event1", GrpcOpenCensusInputTests.DefaultTimeout);

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));
        }

        [TestMethod]
        public async Task GrpcOpenCensusInputTests_ReceivesDataFromMultipleClients()
        {
            // ARRANGE
            int batchesReceived = 0;
            ExportSpanRequest receivedBatch = null;
            
            int port = GetPort();
            var input = new GrpcOpenCensusInput("localhost", port);
            input.Start(exportSpanRequest =>
            {
                Interlocked.Increment(ref batchesReceived);
                receivedBatch = exportSpanRequest;
            });
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));

            // ACT
            ExportSpanRequest batch = new ExportSpanRequest();
            batch.Spans.Add(new Span() {Name = new TruncatableString() {Value = "Event1"}});

            Parallel.For(0, 1000, new ParallelOptions() {MaxDegreeOfParallelism = 1000}, async i =>
            {
                var grpcWriter = new GrpcWriter(false, port);

                await grpcWriter.Write(batch).ConfigureAwait(false);
            });

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1000 && batchesReceived == 1000, GrpcOpenCensusInputTests.DefaultTimeout);

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));
        }

        [TestMethod]
        public async Task GrpcOpenCensusInputTests_StopsWhileWaitingForData()
        {
            // ARRANGE
            int batchesReceived = 0;
            ExportSpanRequest receivedBatch = null;

            int port = GetPort();
            var input = new GrpcOpenCensusInput("localhost", port);

            input.Start(exportSpanRequest =>
            {
                batchesReceived++;
                receivedBatch = exportSpanRequest;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(false, port);

            ExportSpanRequest batch = new ExportSpanRequest();
            batch.Spans.Add(new Span() { Name = new TruncatableString() { Value = "Event1" } });

            await grpcWriter.Write(batch).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Spans.Single().Name.Value == "Event1", GrpcOpenCensusInputTests.DefaultTimeout);

            // ACT
            input.Stop();
            
            // ASSERT
            Common.AssertIsTrueEventually(
                () => !input.IsRunning && input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Spans.Single().Name.Value == "Event1", GrpcOpenCensusInputTests.DefaultTimeout);
        }

        [TestMethod]
        public async Task GrpcOpenCensusInputTests_StopsAndRestarts()
        {
            // ARRANGE
            int batchesReceived = 0;
            ExportSpanRequest receivedBatch = null;

            int port = GetPort();
            var input = new GrpcOpenCensusInput("localhost", port);

            input.Start(exportSpanRequest =>
            {
                batchesReceived++;
                receivedBatch = exportSpanRequest;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(false, port);

            ExportSpanRequest batch = new ExportSpanRequest();
            batch.Spans.Add(new Span() { Name = new TruncatableString() { Value = "Event1" } });

            await grpcWriter.Write(batch).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Spans.Single().Name.Value == "Event1", GrpcOpenCensusInputTests.DefaultTimeout);

            // ACT
            input.Stop();

            Common.AssertIsTrueEventually(
                () => !input.IsRunning && input.GetStats().BatchesReceived == 1 && batchesReceived == 1 &&
                      receivedBatch.Spans.Single().Name.Value == "Event1", GrpcOpenCensusInputTests.DefaultTimeout);

            input.Start(exportSpanRequest =>
            {
                batchesReceived++;
                receivedBatch = exportSpanRequest;
            });

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));

            grpcWriter = new GrpcWriter(false, port);
            batch.Spans.Single().Name.Value = "Event2";
            await grpcWriter.Write(batch).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().BatchesReceived == 1 && batchesReceived == 2 &&
                      receivedBatch.Spans.Single().Name.Value == "Event2", GrpcOpenCensusInputTests.DefaultTimeout);
        }

        [TestMethod]
        public async Task GrpcOpenCensusInputTests_HandlesExceptionsInProcessingHandler()
        {
            // ARRANGE
            int port = GetPort();
            var input = new GrpcOpenCensusInput("localhost", port);

            input.Start(exportSpanRequest => throw new InvalidOperationException());

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcOpenCensusInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(false, port);

            ExportSpanRequest batch = new ExportSpanRequest();
            batch.Spans.Add(new Span() { Name = new TruncatableString() { Value = "Event1" } });

            // ACT
            await grpcWriter.Write(batch).ConfigureAwait(false);

            // ASSERT

            // must have handled the exception by logging it
            // should still be able to process items
            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().BatchesReceived == 0 && input.GetStats().BatchesFailed == 1,
                GrpcOpenCensusInputTests.DefaultTimeout);

            await grpcWriter.Write(batch).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().BatchesReceived == 0 && input.GetStats().BatchesFailed == 2,
                GrpcOpenCensusInputTests.DefaultTimeout);
        }

        private static int GetPort()
        {
            // dynamic port range
            return rand.Next(49152, 65535);
        }
    }
}
