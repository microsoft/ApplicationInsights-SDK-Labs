namespace Microsoft.LocalForwarder.Test.Library
{
    using System;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading;
    using LocalForwarder.Library;
    using VisualStudio.TestTools.UnitTesting;

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
        public void LibraryTests_LibraryProcessesBatchesCorrectly()
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

            
            // ACT
            lib.Stop();

            // ASSERT
            Assert.Fail();
        }
    }
}