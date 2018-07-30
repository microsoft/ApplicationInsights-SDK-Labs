namespace Test.Common
{
    using global::Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.IO;

    [TestClass]
    public class DiagnosticsTests
    {
        [TestMethod]
        public void DiagnosticsTests_LogsMessageToFile()
        {
            // ARRANGE
            File.Delete("LocalForwarder-internal.log");
            File.Delete("LocalForwarder.log");
            var guid = Guid.NewGuid();

            // ACT
            Diagnostics.Log($"Log message here {guid.ToString()}");
            
            // ASSERT
            Assert.IsTrue(File.Exists("LocalForwarder-internal.log"));
            Assert.IsTrue(File.Exists("LocalForwarder.log"));

            Assert.IsTrue(File.ReadAllText("LocalForwarder.log").Contains($"Log message here {guid.ToString()}"));
        }
    }
}
