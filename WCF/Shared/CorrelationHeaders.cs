using System;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Default correlation header names
    /// </summary>
    public static class CorrelationHeaders
    {
        /// <summary>
        /// Default HTTP header name for ParentId
        /// </summary>
        public const String HttpStandardParentIdHeader = "x-ms-request-id";
        /// <summary>
        /// Default HTTP header name for RootId
        /// </summary>
        public const String HttpStandardRootIdHeader = "x-ms-request-root-id";
        /// <summary>
        /// Default SOAP header name for ParentId
        /// </summary>
        public const String SoapStandardParentIdHeader = "requestId";
        /// <summary>
        /// Default SOAP header name for RootId
        /// </summary>
        public const String SoapStandardRootIdHeader = "requestRootId";
        /// <summary>
        /// Default XML namespace for SOAP headers
        /// </summary>
        public const String SoapStandardNamespace = "http://schemas.microsoft.com/application-insights";
    }
}
