using Microsoft.ApplicationInsights.Wcf.Implementation;
using System;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    internal class ClientChannelManager : IChannelManager
    {
        public TimeSpan CloseTimeout { get; private set; }

        public Type ContractType { get; private set; }

        public TimeSpan OpenTimeout { get; private set; }

        public ClientOperationMap OperationMap { get; private set; }

        public TimeSpan ReceiveTimeout { get; private set; }

        public TimeSpan SendTimeout { get; private set; }

        public TelemetryClient TelemetryClient { get; private set; }
        public String RootOperationIdHeaderName { get; set; }
        public String ParentOperationIdHeaderName { get; set; }
        public String SoapRootOperationIdHeaderName { get; set; }
        public String SoapParentOperationIdHeaderName { get; set; }
        public String SoapHeaderNamespace { get; set; }

        public ClientChannelManager(TelemetryClient client, Type contractType, ClientOperationMap map)
        {
            this.TelemetryClient = client;
            this.ContractType = contractType;
            this.OperationMap = map;
            this.CloseTimeout = this.OpenTimeout = this.ReceiveTimeout = this.SendTimeout = TimeSpan.FromSeconds(5);
        }
    }
}
