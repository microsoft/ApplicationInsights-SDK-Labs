namespace MetricsGenerator
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.One;

    class Program
    {
        static void Main(string[] args)
        {
            var telemetryClient = new TelemetryClient();

            telemetryClient.RegisterAggregateMetric("MetricsGenerator", "City", percentileCalculation: PercentileCalculation.OrderByLargest);

            for (double i = -50; i < 10000000; i += new Random().NextDouble())
            {
                telemetryClient.TrackAggregateMetric("MetricsGenerator", i, "Seattle");
                telemetryClient.TrackAggregateMetric("MetricsGenerator", i, "New York");
            }

            Task.Delay(20 * 1000).Wait();
        }
    }
}