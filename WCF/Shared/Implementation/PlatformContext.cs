using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Web;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    /// <summary>
    /// Helper class used to speed up checks for HttpContext
    /// </summary>
    internal static class PlatformContext
    {
        static readonly bool aspNetCompat = ServiceHostingEnvironment.AspNetCompatibilityEnabled;

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
