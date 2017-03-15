namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.ServiceModel.Description;
    using Microsoft.ApplicationInsights.Wcf.Implementation;

    internal class ClientChannelManager : IChannelManager
    {
        public ClientChannelManager(TelemetryClient client, Type contractType)
        {
            this.TelemetryClient = client;
            this.OperationMap = new ClientContract(ContractDescription.GetContract(contractType));
            this.CloseTimeout = this.OpenTimeout = this.ReceiveTimeout = this.SendTimeout = TimeSpan.FromSeconds(5);
        }

        public TimeSpan CloseTimeout { get; private set; }

        public TimeSpan OpenTimeout { get; private set; }

        public ClientContract OperationMap { get; private set; }

        public TimeSpan ReceiveTimeout { get; private set; }

        public TimeSpan SendTimeout { get; private set; }

        public TelemetryClient TelemetryClient { get; private set; }

        public string RootOperationIdHeaderName { get; set; }

        public string ParentOperationIdHeaderName { get; set; }

        public string SoapRootOperationIdHeaderName { get; set; }

        public string SoapParentOperationIdHeaderName { get; set; }

        public string SoapHeaderNamespace { get; set; }
    }
}
