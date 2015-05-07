namespace CounterCollection.Tests
{
    using System;
    using Microsoft.ApplicationInsights.Extensibility.AggregateMetrics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CounterDataTests : AggregationTests
    {
        [TestMethod]
        public void Add()
        {
            var counterData = new MetricsBag();
            
            for (double i = -5; i < 5; i += 0.6)
            {
                counterData.Add(i);
            }
        }

        [TestMethod]
        public void Count()
        {
            var counterData = new MetricsBag();
            const int expectedCount = 5;

            for (int i = 0; i < expectedCount; i++)
            {
                counterData.Add(i);
            }

            AggregationResult agg = counterData.CalculateAggregation();

            Assert.AreEqual(expectedCount, agg.Count);
        }

        [TestMethod]
        public void Sum()
        {
            var counterData = new MetricsBag();
            double expectedSum = 8.61;

            counterData.Add(0.43);
            counterData.Add(1.24);
            counterData.Add(-3.4);
            counterData.Add(10.34);

            AggregationResult agg = counterData.CalculateAggregation();

            Assert.AreEqual(expectedSum, agg.Sum);
        }

        [TestMethod]
        public void Min()
        {
            var counterData = new MetricsBag();
            const double expectedMin = -1.25;

            for (double i = -1.25; i < 6.8; i += 0.33)
            {
                counterData.Add(i);
            }

            AggregationResult agg = counterData.CalculateAggregation();

            Assert.AreEqual(expectedMin, agg.Min);
        }

        [TestMethod]
        public void Max()
        {
            var counterData = new MetricsBag();
            const double expectedMax = 6.67;

            for (double i = -1.25; i < 6.8; i += 0.33)
            {
                counterData.Add(i);
            }

            AggregationResult agg = counterData.CalculateAggregation();

            Assert.AreEqual(expectedMax, Math.Round(agg.Max, 2));
        }

        [TestMethod]
        public void Average()
        {
            var counterData = new MetricsBag();
            const double expectedAverage = 3.33;

            counterData.Add(-5);
            counterData.Add(5);
            counterData.Add(10);

            AggregationResult agg = counterData.CalculateAggregation();

            Assert.AreEqual(expectedAverage, Math.Round(agg.Average, 2));
        }

        [TestMethod]
        public void PercentilesLargest()
        {
            var counterData = new MetricsBag();

            for (int i = 1; i <= 100; i++)
            {
                counterData.Add(i);
            }

            AggregationResult agg = counterData.CalculateAggregation(PercentileCalculation.OrderByLargest);

            Assert.AreEqual(50, agg.P50);
            Assert.AreEqual(75, agg.P75);
            Assert.AreEqual(90, agg.P90);
            Assert.AreEqual(95, agg.P95);
            Assert.AreEqual(99, agg.P99);
        }

        [TestMethod]
        public void PercentilesSmallest()
        {
            var counterData = new MetricsBag();

            for (int i = 1; i <= 100; i++)
            {
                counterData.Add(i);
            }

            AggregationResult agg = counterData.CalculateAggregation(PercentileCalculation.OrderBySmallest);

            Assert.AreEqual(51, agg.P50);
            Assert.AreEqual(26, agg.P75);
            Assert.AreEqual(11, agg.P90);
            Assert.AreEqual(6, agg.P95);
            Assert.AreEqual(2, agg.P99);
        }

        [TestMethod]
        public void CalculateAggregationPerformance()
        {
            var counterData = new MetricsBag();

            const int iterations = 10000000;

            for (int i = 0; i < iterations; i++)
            {
                counterData.Add(i);
            }

            var perfCollector = new PerfCollector(this.TestContext);

            AggregationResult agg = counterData.CalculateAggregation();

            perfCollector.StopAndSubmitPerfData();

            Assert.AreEqual(iterations, agg.Count);
        }

        [TestMethod]
        public void CalculateAggregationPerformanceWithPercentiles()
        {
            var counterData = new MetricsBag();

            const int iterations = 10000000;

            for (int i = 0; i < iterations; i++)
            {
                counterData.Add(i);
            }

            var perfCollector = new PerfCollector(this.TestContext);

            AggregationResult agg = counterData.CalculateAggregation();

            perfCollector.StopAndSubmitPerfData();

            Assert.AreEqual(iterations, agg.Count);
        }
    }
}