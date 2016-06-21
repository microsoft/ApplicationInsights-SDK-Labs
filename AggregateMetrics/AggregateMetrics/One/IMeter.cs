namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One
{
    public interface IMeter
    {
        void Mark();

        void Mark(int count);
    }
}
