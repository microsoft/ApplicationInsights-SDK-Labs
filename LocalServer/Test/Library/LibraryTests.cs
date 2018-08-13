namespace Microsoft.LocalForwarder.Test.Library
{
    using ApplicationInsights;
    using ApplicationInsights.Channel;
    using ApplicationInsights.DataContracts;
    using ApplicationInsights.Extensibility;
    using LocalForwarder.Library;
    using LocalForwarder.Library.Inputs.Contracts;
    using Opencensus.Proto.Exporter;
    using Opencensus.Proto.Trace;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using VisualStudio.TestTools.UnitTesting;
    using Exception = System.Exception;

    [TestClass]
    public class LibraryTests
    {
        private static readonly TimeSpan LongTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public void LibraryTests_LibraryStartsAndStopsWithCorrectConfig()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";
            var lib = new Library(config);

            // ACT
            lib.Run();

            lib.Stop();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LibraryTests_LibraryThrowsOnMalformedConfig()
        {
            // ARRANGE

            // ACT
            new Library("Invalid XML here");

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryThrowsOnInvalidAiPort()
        {
            // ARRANGE
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>NOT_A_NUMBER</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            // ACT
            new Library(config);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryThrowsOnInvalidOcPort()
        {
            // ARRANGE
            int portAI = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>NOT_A_NUMBER</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            // ACT
            new Library(config);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryThrowsOnInvalidAiHost()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>INVALID HOST NAME</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            // ACT
            new Library(config).Run();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryDoesNotStartTwice()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            Library lib = null;

            try
            {
                lib = new Library(config);
                lib.Run();
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            // ACT
            lib.Run();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryDoesNotStopTwice()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            Library lib = null;

            try
            {
                lib = new Library(config);
                lib.Run();
                lib.Stop();
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            // ACT
            lib.Stop();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryDoesNotStopWithoutEverRunning()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            Library lib = null;

            try
            {
                lib = new Library(config);
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            // ACT
            lib.Stop();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryThrowsOnInvalidOcHost()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>INVALID HOST NAME</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            // ACT
            new Library(config).Run();

            // ASSERT
        }

        [TestMethod]
        public void LibraryTests_LibraryTurnsOnBothInputs()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            // ACT
            var lib = new Library(config);
            lib.Run();

            // ASSERT
            // check which ports are in use (listened on)
            var clientAI = new TcpClient("localhost", portAI);
            var clientOC = new TcpClient("localhost", portOC);

            Common.AssertIsTrueEventually(() => clientAI.Connected, LongTimeout);
            Common.AssertIsTrueEventually(() => clientOC.Connected, LongTimeout);

            clientAI.Dispose();
            clientOC.Dispose();
        }

        [TestMethod]
        public void LibraryTests_LibraryTurnsOnAiInputOnly()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""false"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            // ACT
            var lib = new Library(config);
            lib.Run();

            // ASSERT
            // check which ports are in use (listened on)
            var clientAI = new TcpClient("localhost", portAI);
            TcpClient clientOC = null;

            try
            {
                clientOC = new TcpClient("localhost", portOC);
            }
            catch (Exception)
            {
                // swallow
            }

            Common.AssertIsTrueEventually(() => clientAI.Connected, LongTimeout);

            Assert.IsNull(clientOC);

            clientAI.Dispose();
        }

        [TestMethod]
        public void LibraryTests_LibraryTurnsOnOcInputOnly()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""false"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            // ACT
            var lib = new Library(config);
            lib.Run();

            // ASSERT
            // check which ports are in use (listened on)
            var clientOC = new TcpClient("localhost", portOC);
            TcpClient clientAI = null;

            try
            {
                clientAI = new TcpClient("localhost", portAI);
            }
            catch (Exception)
            {
                // swallow
            }

            Common.AssertIsTrueEventually(() => clientOC.Connected, LongTimeout);

            Assert.IsNull(clientAI);

            clientOC.Dispose();
        }

        [TestMethod]
        public void LibraryTests_LibraryTurnsOffBothInputs()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""false"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""false"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            // ACT
            var lib = new Library(config);
            lib.Run();

            // ASSERT
            // check which ports are in use (listened on)
            TcpClient clientAI = null;
            TcpClient clientOC = null;

            try
            {
                clientAI = new TcpClient("localhost", portAI);
            }
            catch (Exception)
            {
                // swallow
            }

            try
            {
                clientOC = new TcpClient("localhost", portOC);
            }
            catch (Exception)
            {
                // swallow
            }

            Assert.IsNull(clientAI);
            Assert.IsNull(clientOC);

            lib.Stop();
        }

        [TestMethod]
        public void LibraryTests_LibraryCleansUpBothInputsWhileStopping()
        {
            // ARRANGE
            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            var lib = new Library(config);
            lib.Run();

            var clientAI = new TcpClient("localhost", portAI);
            var clientOC = new TcpClient("localhost", portOC);

            Common.AssertIsTrueEventually(() => clientAI.Connected && clientOC.Connected, LongTimeout);

            // ACT
            lib.Stop();

            // ASSERT
            // check which ports are in use (listened on)
            clientAI = null;
            clientOC = null;

            try
            {
                clientAI = new TcpClient("localhost", portAI);
            }
            catch (Exception)
            {
                // swallow
            }

            try
            {
                clientOC = new TcpClient("localhost", portOC);
            }
            catch (Exception)
            {
                // swallow
            }

            Assert.IsNull(clientAI);
            Assert.IsNull(clientOC);
        }

        [TestMethod]
        public async Task LibraryTests_LibraryProcessesAiBatchesCorrectly()
        {
            // ARRANGE
            var telemetryClient = Common.SetupStubTelemetryClient(out var sentItems);

            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>ikey1</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            var telemetryBatch = new TelemetryBatch();
            telemetryBatch.Items.Add(new Telemetry() {Event = new Event() {Name = "Event1"}});
            telemetryBatch.Items.Add(new Telemetry() {Message = new Message() {Message_ = "Message1"}});
            telemetryBatch.Items.Add(new Telemetry() {Metric = new LocalForwarder.Library.Inputs.Contracts.Metric() {Metrics = {new DataPoint() {Name = "Metric1", Value = 1}}}});
            telemetryBatch.Items.Add(new Telemetry() {Exception = new LocalForwarder.Library.Inputs.Contracts.Exception() {ProblemId = "Exception1", Exceptions = {new ExceptionDetails() {Message = "Exception1"}}}});
            telemetryBatch.Items.Add(new Telemetry() {Dependency = new Dependency() {Name = "Dependency1"}});
            telemetryBatch.Items.Add(new Telemetry() {Availability = new Availability() {Name = "Availability1"}});
            telemetryBatch.Items.Add(new Telemetry() {PageView = new PageView() {Id = "PageView1"}});
            telemetryBatch.Items.Add(new Telemetry() {Request = new Request() {Name = "Request1"}});

            var lib = new Library(config, telemetryClient);
            lib.Run();

            // ACT
            var writer = new GrpcWriter(true, portAI);
            await writer.Write(telemetryBatch).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(() => sentItems.Count == 8);

            lib.Stop();

            Assert.AreEqual("Event1", (sentItems.Skip(0).First() as EventTelemetry).Name);
            Assert.AreEqual("Message1", (sentItems.Skip(1).First() as TraceTelemetry).Message);
            Assert.AreEqual("Metric1", (sentItems.Skip(2).First() as MetricTelemetry).Name);
            Assert.AreEqual(1, (sentItems.Skip(2).First() as MetricTelemetry).Value);
            Assert.AreEqual("Exception1", (sentItems.Skip(3).First() as ExceptionTelemetry).ProblemId);
            Assert.AreEqual("Exception1", (sentItems.Skip(3).First() as ExceptionTelemetry).ExceptionDetailsInfoList.Single().Message);
            Assert.AreEqual("Dependency1", (sentItems.Skip(4).First() as DependencyTelemetry).Name);
            Assert.AreEqual("Availability1", (sentItems.Skip(5).First() as AvailabilityTelemetry).Name);
            Assert.AreEqual("PageView1", (sentItems.Skip(6).First() as PageViewTelemetry).Id);
            Assert.AreEqual("Request1", (sentItems.Skip(7).First() as RequestTelemetry).Name);
        }

        [TestMethod]
        public async Task LibraryTests_LibraryProcessesOcBatchesCorrectly()
        {
            // ARRANGE
            var telemetryClient = Common.SetupStubTelemetryClient(out var sentItems);

            int portAI = Common.GetPort();
            int portOC = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portAI}</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>0.0.0.0</Host>
      <Port>{portOC}</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>ikey1</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            var telemetryBatch = new ExportSpanRequest();
            telemetryBatch.Spans.Add(new Span() {Name = new TruncatableString() {Value = "Span1"}, Kind = Span.Types.SpanKind.Server});
            telemetryBatch.Spans.Add(new Span() {Name = new TruncatableString() {Value = "Span2"}, Kind = Span.Types.SpanKind.Client});

            var lib = new Library(config, telemetryClient);
            lib.Run();

            // ACT
            var writer = new GrpcWriter(false, portOC);
            await writer.Write(telemetryBatch).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(() => sentItems.Count == 2);

            lib.Stop();

            Assert.AreEqual("Span1", (sentItems.Skip(0).First() as RequestTelemetry).Name);
            Assert.AreEqual("Span2", (sentItems.Skip(1).First() as DependencyTelemetry).Name);
        }
    }
}