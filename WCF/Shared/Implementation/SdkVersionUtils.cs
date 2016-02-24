using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal static class SdkVersionUtils
    {
        public static String GetAssemblyVersion()
        {
            return typeof(SdkVersionUtils).Assembly.GetCustomAttributes(false)
                    .OfType<AssemblyFileVersionAttribute>()
                    .First()
                    .Version;
        }
    }
}
