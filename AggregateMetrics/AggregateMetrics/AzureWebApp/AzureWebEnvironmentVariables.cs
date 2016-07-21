namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.AzureWebApp
{
    using System;

    [Flags]
    public enum AzureWebApEnvironmentVariables
    {
        AspNet = 0,

        App = 1,

        CLR = 2,

        All = 3
    };
}
