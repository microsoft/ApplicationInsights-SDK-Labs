namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    public interface IMeter
    {
        void Mark();

        void Mark(int count);
    }
}
