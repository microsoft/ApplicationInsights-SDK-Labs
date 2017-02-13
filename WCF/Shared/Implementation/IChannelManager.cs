using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal interface IChannelManager : IDefaultCommunicationTimeouts
    {
        TelemetryClient TelemetryClient { get; }
        Type ContractType { get; }
        ClientOperationMap OperationMap { get; }
        String RootOperationIdHeaderName { get; }
        String ParentOperationIdHeaderName { get; }
        String SoapRootOperationIdHeaderName { get; }
        String SoapParentOperationIdHeaderName { get; }
        String SoapHeaderNamespace { get; }

    }
}
