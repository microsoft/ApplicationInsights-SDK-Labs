namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    public interface IHistogram
    {
        void Update(int value);
    }
}
