namespace Microsoft.LocalForwarder.Test.Library
{
    using System;
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

        [TestMethod]
        public void ConfigurationTests_EnvironmentVariablesAreResolved()
        {
            // ARRANGE
            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>%AI_Host%</Host>
      <Port>%AI_Port%</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>%OC_Host%</Host>
      <Port>%OC_Port%</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>%ConfigTestInstrumentationKey%</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            var rand = new Random();
            string aiHost = Guid.NewGuid().ToString();
            string aiPort = rand.Next().ToString();
            string ocHost = Guid.NewGuid().ToString();
            string ocPort = rand.Next().ToString();
            string ikey = Guid.NewGuid().ToString();

            Environment.SetEnvironmentVariable("AI_Host", aiHost);
            Environment.SetEnvironmentVariable("AI_Port", aiPort);
            Environment.SetEnvironmentVariable("OC_Host", ocHost);
            Environment.SetEnvironmentVariable("OC_Port", ocPort);
            Environment.SetEnvironmentVariable("ConfigTestInstrumentationKey", ikey);

            // ACT
            var configuration = new Configuration(config);

            // ASSERT
            Assert.AreEqual(aiHost, configuration.ApplicationInsightsInput_Host);
            Assert.AreEqual(aiPort, configuration.ApplicationInsightsInput_Port.ToString());
            Assert.AreEqual(ocHost, configuration.OpenCensusInput_Host);
            Assert.AreEqual(ocPort, configuration.OpenCensusInput_Port.ToString());
            Assert.AreEqual(ikey, configuration.OpenCensusToApplicationInsights_InstrumentationKey);
        }

        [TestMethod]
        public void ConfigurationTests_EnvironmentVariablesAreNotResolvedIfNonExistent()
        {
            // ARRANGE
            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<LocalForwarderConfiguration>
  <Inputs>
    <ApplicationInsightsInput Enabled=""true"">
      <Host>%AI_Host%</Host>
      <Port>0</Port>
    </ApplicationInsightsInput>
    <OpenCensusInput Enabled=""true"">
      <Host>%OC_Host%</Host>
      <Port>0</Port>
    </OpenCensusInput>
  </Inputs>
  <OpenCensusToApplicationInsights>
    <InstrumentationKey>%ConfigTestInstrumentationKey%</InstrumentationKey>
  </OpenCensusToApplicationInsights>
</LocalForwarderConfiguration>
";

            string aiHost = Guid.NewGuid().ToString();
            string ocHost = Guid.NewGuid().ToString();
            string ikey = Guid.NewGuid().ToString();

            Environment.SetEnvironmentVariable("AI_Host", null);
            Environment.SetEnvironmentVariable("OC_Host", null);
            Environment.SetEnvironmentVariable("ConfigTestInstrumentationKey", null);

            // ACT
            var configuration = new Configuration(config);

            // ASSERT
            Assert.AreEqual("%AI_Host%", configuration.ApplicationInsightsInput_Host);
            Assert.AreEqual("%OC_Host%", configuration.OpenCensusInput_Host);
            Assert.AreEqual("%ConfigTestInstrumentationKey%", configuration.OpenCensusToApplicationInsights_InstrumentationKey);
        }
    }
}