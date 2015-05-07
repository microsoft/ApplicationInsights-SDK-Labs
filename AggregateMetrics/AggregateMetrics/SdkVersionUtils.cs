namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics
{
    using System.Linq;
    using System.Reflection;

    internal class SdkVersionUtils
    {
        internal const string VersionPrefix = "metrics-aggs: ";

        internal static string GetAssemblyVersion()
        {
            return typeof(SdkVersionUtils).GetTypeInfo().Assembly.GetCustomAttributes<AssemblyFileVersionAttribute>()
                    .First()
                    .Version;
        }
    }
}