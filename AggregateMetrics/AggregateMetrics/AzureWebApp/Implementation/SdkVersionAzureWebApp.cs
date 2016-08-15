namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    internal class SdkVersionAzureWebApp
    {
        internal static string sdkVersionAzureWebApp = VersionPrefix + GetAssemblyVersion();

        internal const string VersionPrefix = "azwapc: ";

        internal static string GetAssemblyVersion()
        {
            return SdkVersionUtils.GetAssemblyVersion();
        }
    }
}