using Microsoft.ApplicationInsights.Wcf.Implementation;
using System;
using System.ServiceModel.Description;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    internal class ClientChannelManager : IChannelManager
    {
        public TimeSpan CloseTimeout { get; private set; }

        public TimeSpan OpenTimeout { get; private set; }

        public ClientContract OperationMap { get; private set; }

        public TimeSpan ReceiveTimeout { get; private set; }

        public TimeSpan SendTimeout { get; private set; }

        public TelemetryClient TelemetryClient { get; private set; }
        public String RootOperationIdHeaderName { get; set; }
        public String ParentOperationIdHeaderName { get; set; }
        public String SoapRootOperationIdHeaderName { get; set; }
        public String SoapParentOperationIdHeaderName { get; set; }
        public String SoapHeaderNamespace { get; set; }

        public ClientChannelManager(TelemetryClient client, Type contractType)
        {
            this.TelemetryClient = client;
            this.OperationMap = new ClientContract(ContractDescription.GetContract(contractType));
            this.CloseTimeout = this.OpenTimeout = this.ReceiveTimeout = this.SendTimeout = TimeSpan.FromSeconds(5);
        }
    }
}
