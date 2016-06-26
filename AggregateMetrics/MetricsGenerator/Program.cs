namespace MetricsGenerator
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two;
    using System.Collections.Concurrent;
    using System.Threading;
    class Program
    {
        static void Main(string[] args)
        {
            var telemetryClient = new TelemetryClient();

            
            //API one:

            //telemetryClient.RegisterAggregateMetric("MetricsGenerator", "City", percentileCalculation: PercentileCalculation.OrderByLargest);

            //for (double i = -50; i < 10000000; i += new Random().NextDouble())
            //{
            //    telemetryClient.TrackAggregateMetric("MetricsGenerator", i, "Seattle");
            //    telemetryClient.TrackAggregateMetric("MetricsGenerator", i, "New York");
            //}


            //API two:

            Random rand = new Random();
            telemetryClient.Gauge("active processes", () => { return rand.Next(10, 15); });

            var counter = telemetryClient.Counter("# of items");

            var meter = telemetryClient.Meter("rate of items");
            var aMeter = telemetryClient.Meter("rate of char a");

            while (true)
            {
                var queue = new ConcurrentQueue<char>();
                var a = Console.ReadKey().KeyChar;
                queue.Enqueue(a);
                counter.Increment();
                meter.Mark();
                if (a == 'a')
                {
                    aMeter.Mark();
                }
                else
                {
                    aMeter.Mark(0);
                }

                new Task(() =>
                {
                    Thread.Sleep(100);
                    char ch;
                    if (queue.TryDequeue(out ch))
                    {
                        counter.Decrement();
                        Console.Write(ch);
                    }
                }).Start();
            }
            
        }
    }
}