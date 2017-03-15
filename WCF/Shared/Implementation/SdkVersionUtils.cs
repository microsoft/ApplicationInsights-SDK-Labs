namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Linq;
    using System.Reflection;

    internal static class SdkVersionUtils
    {
        public static string GetAssemblyVersion()
        {
            return typeof(SdkVersionUtils).Assembly.GetCustomAttributes(false)
                    .OfType<AssemblyFileVersionAttribute>()
                    .First()
                    .Version;
        }
    }
}
