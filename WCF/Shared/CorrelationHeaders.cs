namespace Microsoft.ApplicationInsights.Wcf
{
    using System;

    /// <summary>
    /// Default correlation header names.
    /// </summary>
    public static class CorrelationHeaders
    {
        /// <summary>
        /// Default HTTP header name for ParentId.
        /// </summary>
        public const string HttpStandardParentIdHeader = "x-ms-request-id";

        /// <summary>
        /// Default HTTP header name for RootId.
        /// </summary>
        public const string HttpStandardRootIdHeader = "x-ms-request-root-id";

        /// <summary>
        /// Default SOAP header name for ParentId.
        /// </summary>
        public const string SoapStandardParentIdHeader = "requestId";

        /// <summary>
        /// Default SOAP header name for RootId.
        /// </summary>
        public const string SoapStandardRootIdHeader = "requestRootId";

        /// <summary>
        /// Default XML namespace for SOAP headers.
        /// </summary>
        public const string SoapStandardNamespace = "http://schemas.microsoft.com/application-insights";
    }
}
