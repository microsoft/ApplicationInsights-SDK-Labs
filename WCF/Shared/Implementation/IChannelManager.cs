using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal interface IChannelManager : IDefaultCommunicationTimeouts
    {
        TelemetryClient TelemetryClient { get; }
        Type ContractType { get; }
        ClientOperationMap OperationMap { get; }
    }
}
