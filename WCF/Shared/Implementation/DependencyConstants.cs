namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;

    internal static class DependencyConstants
    {
        public const string WcfClientCall = "WCF Service Call";
        public const string WcfChannelOpen = "WCF Channel Open";

        public const string IsOneWayProperty = "isOneWay";
        public const string SoapActionProperty = "soapAction";
    }
}
