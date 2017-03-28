namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.Web;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Helper class used to speed up checks for RequestTelemetry
    /// created by the Web SDK, which is stored in HttpContext.
    /// </summary>
    internal static class PlatformContext
    {
        private static readonly bool AspNetCompat = ServiceHostingEnvironment.AspNetCompatibilityEnabled;

        internal static RequestTelemetry RequestFromHttpContext()
        {
            // only check HttpContext if WCF hosting is configured with
            // <serviceHostingEnvironment aspNetCompatibilityEnabled="true" />
            // See: https://msdn.microsoft.com/en-us/library/aa702682(v=vs.110).aspx
            if (AspNetCompat)
            {
                return GetRequestTelemetryFromHttpContext();
            }

            return null;
        }

        internal static HttpContext GetContext()
        {
            // only check HttpContext if WCF hosting is configured with
            // <serviceHostingEnvironment aspNetCompatibilityEnabled="true" />
            // See: https://msdn.microsoft.com/en-us/library/aa702682(v=vs.110).aspx
            if (AspNetCompat)
            {
                return HttpContext.Current;
            }

            return null;
        }

        // prevent method from being inlined so that JIT
        // doesn't try loading System.Web unless we're being called
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static RequestTelemetry GetRequestTelemetryFromHttpContext()
        {
            var context = HttpContext.Current;
            if (context != null)
            {
                return context.Items[RequestTrackingConstants.HttpContextRequestTelemetryName]
                    as RequestTelemetry;
            }

            return null;
        }
    }
}
