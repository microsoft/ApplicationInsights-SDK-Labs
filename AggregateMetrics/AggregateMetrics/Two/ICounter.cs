namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    public interface ICounter
    {
        void Increment();

        void Increment(long value);

        void Decrement();

        void Decrement(long value);
    }
}
