namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.ServiceModel.Description;
    using Microsoft.ApplicationInsights.Wcf.Implementation;
    using Microsoft.ApplicationInsights.Wcf.Tests.Service;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClientContractTests
    {
        [TestMethod]
        public void CanBuildDescriptionForTwoWayContract()
        {
            var cd = ContractDescription.GetContract(typeof(ISimpleService));
            var cc = new ClientContract(cd);
            Assert.AreEqual(typeof(ISimpleService), cc.ContractType);

            ClientOperation op;
            var found = cc.TryLookupByAction("http://tempuri.org/ISimpleService/GetSimpleData", out op);
            Assert.IsTrue(found);
            Assert.AreEqual(false, op.IsOneWay);
            Assert.AreEqual("ISimpleService.GetSimpleData", op.Name);
        }

        [TestMethod]
        public void CanBuildDescriptionForOneWayContract()
        {
            var cd = ContractDescription.GetContract(typeof(IOneWayService));
            var cc = new ClientContract(cd);
            Assert.AreEqual(typeof(IOneWayService), cc.ContractType);

            ClientOperation op;
            var found = cc.TryLookupByAction("http://tempuri.org/IOneWayService/SuccessfullOneWayCall", out op);
            Assert.IsTrue(found);
            Assert.AreEqual(true, op.IsOneWay);
            Assert.AreEqual("IOneWayService.SuccessfullOneWayCall", op.Name);
        }
    }
}
