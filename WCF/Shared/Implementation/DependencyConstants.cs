using System;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal static class DependencyConstants
    {
        public const String WcfClientCall = "WCF Service Call";
        public const String WcfChannelOpen = "WCF Channel Open";

        public const String IsOneWayProperty = "isOneWay";
        public const String SoapActionProperty = "soapAction";
    }
}
