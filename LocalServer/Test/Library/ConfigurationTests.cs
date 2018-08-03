namespace Microsoft.LocalForwarder.Test.Library
{
    using System.IO;
    using System.Reflection;
    using LocalForwarder.Library;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void ConfigurationTests_DefaultConfigurationIsCorrect()
        {
            // ARRANGE
            string defaultConfig;
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Microsoft.LocalForwarder.Test.LocalForwarder.config";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    defaultConfig = reader.ReadToEnd();
                }
            }

            // ACT
            var config = new Configuration(defaultConfig);

            // ASSERT
            Assert.AreEqual(true, config.ApplicationInsightsInput_Enabled);
            Assert.AreEqual("0.0.0.0", config.ApplicationInsightsInput_Host);
            Assert.AreEqual(50001, config.ApplicationInsightsInput_Port);

            Assert.AreEqual(true, config.OpenCensusInput_Enabled);
            Assert.AreEqual("0.0.0.0", config.OpenCensusInput_Host);
            Assert.AreEqual(50002, config.OpenCensusInput_Port);

            Assert.AreEqual("[SPECIFY INSTRUMENTATION KEY HERE]", config.OpenCensusToApplicationInsights_InstrumentationKey);
        }
    }
}