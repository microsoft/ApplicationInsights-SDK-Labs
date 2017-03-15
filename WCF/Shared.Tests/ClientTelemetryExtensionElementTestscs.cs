namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.Configuration;
    using System.Linq;
    using Microsoft.ApplicationInsights.Wcf.Implementation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClientTelemetryExtensionElementTestscs
    {
        [TestMethod]
        [TestCategory("Client")]
        public void WhenCreated_PropertiesReturnsAll()
        {
            var element = new ClientTelemetryExtensionElement();
            var props = element.CreateProperties();
            Assert.AreEqual(5, props.Count);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenCreated_BehaviorTypeReturnsRightValue()
        {
            var element = new ClientTelemetryExtensionElement();
            Assert.AreEqual(typeof(ClientTelemetryEndpointBehavior), element.BehaviorType);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenCreated_PropertiesHaveDefaultValues()
        {
            var element = new ClientTelemetryExtensionElement();
            var props = element.CreateProperties();

            var prop = props.OfType<ConfigurationProperty>().First(x => x.Name == "rootOperationIdHeaderName");
            Assert.AreEqual(CorrelationHeaders.HttpStandardRootIdHeader, prop.DefaultValue);

            prop = props.OfType<ConfigurationProperty>().First(x => x.Name == "parentOperationIdHeaderName");
            Assert.AreEqual(CorrelationHeaders.HttpStandardParentIdHeader, prop.DefaultValue);

            prop = props.OfType<ConfigurationProperty>().First(x => x.Name == "soapRootOperationIdHeaderName");
            Assert.AreEqual(CorrelationHeaders.SoapStandardRootIdHeader, prop.DefaultValue);

            prop = props.OfType<ConfigurationProperty>().First(x => x.Name == "soapParentOperationIdHeaderName");
            Assert.AreEqual(CorrelationHeaders.SoapStandardParentIdHeader, prop.DefaultValue);

            prop = props.OfType<ConfigurationProperty>().First(x => x.Name == "soapHeaderNamespace");
            Assert.AreEqual(CorrelationHeaders.SoapStandardNamespace, prop.DefaultValue);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenCreated_TypePropertiesHaveDefaultValues()
        {
            var element = new ClientTelemetryExtensionElement();

            Assert.AreEqual(CorrelationHeaders.HttpStandardRootIdHeader, element.RootOperationIdHeaderName);
            Assert.AreEqual(CorrelationHeaders.HttpStandardParentIdHeader, element.ParentOperationIdHeaderName);
            Assert.AreEqual(CorrelationHeaders.SoapStandardRootIdHeader, element.SoapRootOperationIdHeaderName);
            Assert.AreEqual(CorrelationHeaders.SoapStandardParentIdHeader, element.SoapParentOperationIdHeaderName);
            Assert.AreEqual(CorrelationHeaders.SoapStandardNamespace, element.SoapHeaderNamespace);
        }

        [TestMethod]
        [TestCategory("Client")]
        public void WhenCreateBehaviorIsCalled_BehaviorHasPropertiesSet()
        {
            var element = new ClientTelemetryExtensionElement();
            element.ParentOperationIdHeaderName = "myParentId";
            element.RootOperationIdHeaderName = "myRootId";
            element.SoapParentOperationIdHeaderName = "soapMyParentId";
            element.SoapRootOperationIdHeaderName = "soapMyRootId";
            element.SoapHeaderNamespace = "urn:soapheader";

            var behavior = element.CreateBehaviorInternal();
            Assert.AreEqual(element.ParentOperationIdHeaderName, behavior.ParentOperationIdHeaderName);
            Assert.AreEqual(element.RootOperationIdHeaderName, behavior.RootOperationIdHeaderName);
            Assert.AreEqual(element.SoapParentOperationIdHeaderName, behavior.SoapParentOperationIdHeaderName);
            Assert.AreEqual(element.SoapRootOperationIdHeaderName, behavior.SoapRootOperationIdHeaderName);
            Assert.AreEqual(element.SoapHeaderNamespace, behavior.SoapHeaderNamespace);
        }
    }
}
