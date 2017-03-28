namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel;

    internal interface IChannelManager : IDefaultCommunicationTimeouts
    {
        TelemetryClient TelemetryClient { get; }

        ClientContract OperationMap { get; }

        string RootOperationIdHeaderName { get; }

        string ParentOperationIdHeaderName { get; }

        string SoapRootOperationIdHeaderName { get; }

        string SoapParentOperationIdHeaderName { get; }

        string SoapHeaderNamespace { get; }
    }
}
