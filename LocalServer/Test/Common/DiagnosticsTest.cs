namespace Microsoft.LocalForwarder.Test
{
    using System;
    using System.IO;
    using System.Threading;
    using LocalForwarder.Common;
    using NLog;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DiagnosticsTests
    {
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public void DiagnosticsTests_LogsMessageToFile()
        {
            // ARRANGE
            var guid = Guid.NewGuid();

            // ACT
            string testLogMessage = $"Log message here {guid.ToString()}";
            Diagnostics.LogTrace(testLogMessage);

            // ASSERT
            Diagnostics.Shutdown(TimeSpan.FromSeconds(1));
            Thread.Sleep(TimeSpan.FromSeconds(1));

            Assert.IsTrue(SpinWait.SpinUntil(() => File.Exists("LocalForwarder-internal.log"), this.timeout));
            Assert.IsTrue(SpinWait.SpinUntil(() => File.Exists("LocalForwarder.log"), this.timeout));
            Assert.IsTrue(File.ReadAllText("LocalForwarder.log").Contains(testLogMessage));
        }
    }
}
