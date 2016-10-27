using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Web;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    /// <summary>
    /// Helper class used to speed up checks for RequestTelemetry
    /// created by the Web SDK, which is stored in HttpContext
    /// </summary>
    internal static class PlatformContext
    {
        static readonly bool aspNetCompat = ServiceHostingEnvironment.AspNetCompatibilityEnabled;

        internal static RequestTelemetry RequestFromHttpContext()
        {
            // only check HttpContext if WCF hosting is configured with
            // <serviceHostingEnvironment aspNetCompatibilityEnabled="true" />
            // See: https://msdn.microsoft.com/en-us/library/aa702682(v=vs.110).aspx
            if ( aspNetCompat )
            {
                return GetRequestTelemetryFromHttpContext();
            }
            return null;
        }

        // prevent method from being inlined so that JIT
        // doesn't try loading System.Web unless we're being called
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static RequestTelemetry GetRequestTelemetryFromHttpContext()
        {
            var context = HttpContext.Current;
            if ( context != null )
            {
                return context.Items[RequestTrackingConstants.HttpContextRequestTelemetryName]
                    as RequestTelemetry;
            }
            return null;
        }
        /// <summary>
        /// Returns the HttpContext for the current thread
        /// </summary>
        /// <returns>Null if not running on IIS or not available</returns>
        internal static HttpContext GetContext()
        {
            // only check HttpContext if WCF hosting is configured with
            // <serviceHostingEnvironment aspNetCompatibilityEnabled="true" />
            // See: https://msdn.microsoft.com/en-us/library/aa702682(v=vs.110).aspx
            if ( aspNetCompat )
            {
                return HttpContext.Current;
            }
            return null;
        }
    }
}
