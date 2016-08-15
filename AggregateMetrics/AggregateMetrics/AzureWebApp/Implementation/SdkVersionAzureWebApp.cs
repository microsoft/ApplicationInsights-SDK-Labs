namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    internal class SdkVersionAzureWebApp
    {
        internal const string VersionPrefix = "azwpac: ";

        internal static string GetAssemblyVersion()
        {
            return SdkVersionUtils.GetAssemblyVersion();
        }
    }
}